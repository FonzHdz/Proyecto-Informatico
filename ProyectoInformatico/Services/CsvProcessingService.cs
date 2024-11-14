using CsvHelper;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

public class CsvProcessingService
{
    public List<T> ProcessCsvFile<T>(Stream csvStream)
    {
        using var reader = new StreamReader(csvStream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        return csv.GetRecords<T>().ToList();
    }
}