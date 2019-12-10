using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace WebCsvParser.Helper
{
    public static class SqlBulkHelper
    {
        public static void BulkInsert<T>(string connectionString, IEnumerable<T> list, int batchSize = 0, string table = null)
        {
            using (var bulkCopy = new SqlBulkCopy(connectionString))
            {
                var type = typeof(T);

                var tableName = type.Name;
                var tableAttribute = (TableAttribute)type.GetCustomAttributes(typeof(TableAttribute), false).FirstOrDefault();
                if (tableAttribute != null)
                {
                    var schemaName = string.IsNullOrWhiteSpace(tableAttribute.Schema) ? tableAttribute.Schema + "." : "";
                    tableName = schemaName + tableAttribute.Name;
                }
                if (table != null) tableName = table;

                bulkCopy.BatchSize = batchSize;
                bulkCopy.DestinationTableName = tableName;

                var dataTable = new DataTable();
                var props = TypeDescriptor.GetProperties(typeof(T))
                                           //Dirty hack to make sure we only have system data types 
                                           //i.e. filter out the relationships/collections
                                           .Cast<PropertyDescriptor>()
                                           .Where(propertyInfo => propertyInfo.PropertyType.Namespace.Equals("System"))
                                           .Where(i => !i.Attributes.OfType<DatabaseGeneratedAttribute>().Any())
                                           .Where(i => !i.Attributes.OfType<NotMappedAttribute>().Any())
                                           .ToArray();

                foreach (var propertyInfo in props)
                {
                    var colName = propertyInfo.Name;

                    var colAttribute = propertyInfo.Attributes.OfType<ColumnAttribute>().FirstOrDefault();
                    if (colAttribute != null) colName = colAttribute.Name;

                    bulkCopy.ColumnMappings.Add(propertyInfo.Name, colName);
                    dataTable.Columns.Add(propertyInfo.Name, Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType);
                }

                var values = new object[props.Length];
                foreach (var item in list)
                {
                    for (var i = 0; i < values.Length; i++)
                        values[i] = props[i].GetValue(item);

                    dataTable.Rows.Add(values);
                }

                bulkCopy.WriteToServer(dataTable);
            }
        }

        public static void BulkInsert<T>(this DbContext context, IEnumerable<T> list, int batchSize = 0, string table = null)
        {
            BulkInsert(context.Database.GetDbConnection().ConnectionString, list, batchSize, table);
        }
    }
}
