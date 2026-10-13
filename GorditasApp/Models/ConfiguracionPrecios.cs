namespace GorditasApp.Models;

public class ConfiguracionPrecios
{
    public decimal PrecioGordita { get; set; } = 15m;
    public decimal PrecioTaco { get; set; } = 15m;
    public decimal CostoRefrescoTotal { get; set; } = 0m;
    public decimal CostoEnvioTotal { get; set; } = 0m;

    public decimal PrecioUnitario(TipoAlimento tipo) =>
        tipo == TipoAlimento.Gordita ? PrecioGordita : PrecioTaco;
}
