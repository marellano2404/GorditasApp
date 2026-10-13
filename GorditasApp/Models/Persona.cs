namespace GorditasApp.Models;

public class Persona
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public bool EsCumpleanero { get; set; }
    public bool CooperaCumpleanero { get; set; } = true;
    public bool CooperaRefresco { get; set; } = true;
    public bool CooperaEnvio { get; set; } = true;
    public List<ItemPedido> Items { get; set; } = new();

    // Cuánto entregó esta persona en efectivo para pagar su cuenta.
    public decimal MontoPagado { get; set; }

    public decimal CalcularCostoPedido(ConfiguracionPrecios precios) =>
        Items.Sum(i => i.Cantidad * precios.PrecioUnitario(i.Tipo));
}
