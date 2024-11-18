using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

public class CsvProcessingService
{
    public List<T> ProcessCsvFile<T>(Stream csvStream, string encoding = "UTF-8")
    {
        try
        {
            // Configuración de codificación
            using var reader = new StreamReader(csvStream, System.Text.Encoding.GetEncoding(encoding));

            // Configuración del CsvHelper
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null, // Ignorar campos faltantes
                HeaderValidated = null,  // Ignorar validaciones del encabezado
                BadDataFound = context =>
                {
                    Console.WriteLine($"Dato inválido detectado: {context.RawRecord}");
                }
            };

            using var csv = new CsvReader(reader, config);

            // Normalización de caracteres especiales
            var records = csv.GetRecords<T>()
                .Select(record =>
                {
                    NormalizeProperties(record);
                    return record;
                }).ToList();

            return records;
        }
        catch (CsvHelperException ex)
        {
            Console.WriteLine($"Error al procesar el archivo CSV: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error general: {ex.Message}");
            throw;
        }
    }

    private void NormalizeProperties<T>(T record)
    {
        foreach (var property in typeof(T).GetProperties())
        {
            if (property.PropertyType == typeof(string))
            {
                var value = property.GetValue(record) as string;
                if (value != null)
                {
                    property.SetValue(record, value.Normalize(System.Text.NormalizationForm.FormC));
                }
            }
        }
    }
}