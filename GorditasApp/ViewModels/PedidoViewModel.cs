using System.ComponentModel;
using GorditasApp.Models;

namespace GorditasApp.ViewModels;

public class PedidoViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private void Notify(string prop) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

    public ConfiguracionPrecios Precios { get; } = new();
    public List<Persona> Personas { get; } = new();
    public List<ResultadoPersona> Resultados { get; private set; } = new();

    public decimal TotalRecaudado => Math.Round(Resultados.Sum(r => r.TotalAPagar), 2);
    public decimal GastoReal => Math.Round(Personas.Sum(p => p.CalcularCostoPedido(Precios)) + Precios.CostoRefrescoTotal + Precios.CostoEnvioTotal, 2);

    public void ActualizarPrecios(decimal precioGordita, decimal precioTaco, decimal costoRefresco, decimal costoEnvio)
    {
        Precios.PrecioGordita = precioGordita;
        Precios.PrecioTaco = precioTaco;
        Precios.CostoRefrescoTotal = costoRefresco;
        Precios.CostoEnvioTotal = costoEnvio;
        Notify(nameof(Precios));
    }

    public Persona AgregarPersona(string nombre, bool esCumpleanero = false)
    {
        var persona = new Persona { Nombre = nombre, EsCumpleanero = esCumpleanero };
        Personas.Add(persona);
        Notify(nameof(Personas));
        return persona;
    }

    public void EliminarPersona(Guid personaId)
    {
        Personas.RemoveAll(p => p.Id == personaId);
        Notify(nameof(Personas));
    }

    public void AgregarItem(Guid personaId, TipoAlimento tipo, TipoRelleno relleno, int cantidad)
    {
        var persona = Personas.FirstOrDefault(p => p.Id == personaId);
        if (persona is null || cantidad <= 0) return;

        persona.Items.Add(new ItemPedido { Tipo = tipo, Relleno = relleno, Cantidad = cantidad });
        Notify(nameof(Personas));
    }

    public void EliminarItem(Guid personaId, Guid itemId)
    {
        var persona = Personas.FirstOrDefault(p => p.Id == personaId);
        persona?.Items.RemoveAll(i => i.Id == itemId);
        Notify(nameof(Personas));
    }

    /// <summary>
    /// Registra cuánto pagó en efectivo una persona. Actualiza tanto a la persona
    /// (para que no se pierda si se vuelve a calcular la cuenta) como el resultado
    /// ya calculado, sin necesidad de presionar "Calcular" de nuevo.
    /// </summary>
    public void ActualizarMontoPagado(Guid personaId, decimal monto)
    {
        var persona = Personas.FirstOrDefault(p => p.Id == personaId);
        if (persona is null) return;

        persona.MontoPagado = monto;

        var resultado = Resultados.FirstOrDefault(r => r.PersonaId == personaId);
        if (resultado is not null)
        {
            resultado.MontoPagado = monto;
            Notify(nameof(Resultados));
        }
    }

    public List<ResultadoPersona> Calcular()
    {
        var costoPorPersona = Personas.ToDictionary(p => p.Id, p => p.CalcularCostoPedido(Precios));

        decimal costoCumpleaneros = Personas
            .Where(p => p.EsCumpleanero)
            .Sum(p => costoPorPersona[p.Id]);

        var cooperantesCumple = Personas
            .Where(p => !p.EsCumpleanero && p.CooperaCumpleanero)
            .ToList();

        var cooperantesRefresco = Personas.Where(p => p.CooperaRefresco).ToList();
        var cooperantesEnvio = Personas.Where(p => p.CooperaEnvio).ToList();

        // Se reparte el monto exacto (centavo por centavo) entre los cooperantes, de forma
        // que la suma de lo repartido SIEMPRE sea igual al monto total (sin perder ni ganar
        // centavos por el redondeo de divisiones como $50 / 3 = $16.666...).
        var aportacionCumplePorPersona = RepartirEntre(costoCumpleaneros, cooperantesCumple);
        var aportacionRefrescoPorPersona = RepartirEntre(Precios.CostoRefrescoTotal, cooperantesRefresco);
        var aportacionEnvioPorPersona = RepartirEntre(Precios.CostoEnvioTotal, cooperantesEnvio);

        var resultados = new List<ResultadoPersona>();

        foreach (var persona in Personas)
        {
            var costoPedido = costoPorPersona[persona.Id];

            var resultado = new ResultadoPersona
            {
                PersonaId = persona.Id,
                Nombre = persona.Nombre,
                EsCumpleanero = persona.EsCumpleanero,
                DetallePedido = persona.Items.Count == 0
                    ? "(sin pedido)"
                    : string.Join(", ", persona.Items.Select(i => i.Descripcion)),
                CostoPedido = Math.Round(costoPedido, 2, MidpointRounding.AwayFromZero),
                PagaPorSuPedido = persona.EsCumpleanero
                    ? 0m
                    : Math.Round(costoPedido, 2, MidpointRounding.AwayFromZero),
                AportacionCumpleanero = aportacionCumplePorPersona.GetValueOrDefault(persona.Id, 0m),
                AportacionRefresco = aportacionRefrescoPorPersona.GetValueOrDefault(persona.Id, 0m),
                AportacionEnvio = aportacionEnvioPorPersona.GetValueOrDefault(persona.Id, 0m),
                MontoPagado = persona.MontoPagado
            };

            resultados.Add(resultado);
        }

        Resultados = resultados;
        Notify(nameof(Resultados));
        Notify(nameof(TotalRecaudado));
        Notify(nameof(GastoReal));
        return Resultados;
    }

    /// <summary>
    /// Reparte "total" entre "cooperantes" en partes iguales redondeadas a 2 decimales,
    /// ajustando la última parte para que la suma de todas cierre exacto contra el total
    /// redondeado (evita diferencias de 1 centavo por divisiones no exactas, ej. 50/3).
    /// </summary>
    private static Dictionary<Guid, decimal> RepartirEntre(decimal total, List<Persona> cooperantes)
    {
        var resultado = new Dictionary<Guid, decimal>();
        if (cooperantes.Count == 0) return resultado;

        decimal totalRedondeado = Math.Round(total, 2, MidpointRounding.AwayFromZero);
        decimal montoBase = Math.Round(total / cooperantes.Count, 2, MidpointRounding.AwayFromZero);

        decimal acumulado = 0m;
        for (int i = 0; i < cooperantes.Count; i++)
        {
            if (i == cooperantes.Count - 1)
            {
                // al ultimo cooperante le toca lo que falte para cerrar exacto (ajuste de centavos)
                resultado[cooperantes[i].Id] = Math.Round(totalRedondeado - acumulado, 2, MidpointRounding.AwayFromZero);
            }
            else
            {
                resultado[cooperantes[i].Id] = montoBase;
                acumulado += montoBase;
            }
        }

        return resultado;
    }
}
