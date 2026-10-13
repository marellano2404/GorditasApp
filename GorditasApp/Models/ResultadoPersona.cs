namespace GorditasApp.Models;

public class ResultadoPersona
{
    public Guid PersonaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool EsCumpleanero { get; set; }
    public string DetallePedido { get; set; } = string.Empty;
    public decimal CostoPedido { get; set; }
    public decimal PagaPorSuPedido { get; set; }
    public decimal AportacionCumpleanero { get; set; }
    public decimal AportacionRefresco { get; set; }
    public decimal AportacionEnvio { get; set; }

    // Cuánto entregó esta persona en efectivo.
    public decimal MontoPagado { get; set; }

    public decimal TotalAPagar => Math.Round(PagaPorSuPedido + AportacionCumpleanero + AportacionRefresco + AportacionEnvio, 2);

    // Positivo = se le debe regresar cambio. Negativo = todavía debe esa cantidad.
    public decimal Cambio => Math.Round(MontoPagado - TotalAPagar, 2);
}
