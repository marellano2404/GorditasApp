window.gorditasApp = {
    imprimir: function () {
        window.print();
    },
    descargarCsv: function (nombreArchivo, contenido) {
        // Se agrega un BOM UTF-8 para que Excel muestre bien acentos y "ñ".
        const bom = "\uFEFF";
        const blob = new Blob([bom + contenido], { type: "text/csv;charset=utf-8;" });
        const url = URL.createObjectURL(blob);
        const a = document.createElement("a");
        a.href = url;
        a.download = nombreArchivo;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    },
    // Cuenta cuántas veces se ha abierto la app EN ESTE navegador/dispositivo
    // (no hay login ni base de datos, así que no se puede contar por "usuario" real).
    contarVisita: function () {
        const clave = "gorditasApp_visitas";
        let visitas = parseInt(localStorage.getItem(clave) || "0", 10);
        visitas += 1;
        localStorage.setItem(clave, visitas.toString());
        return visitas;
    }
};
