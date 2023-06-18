using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Data;
using System.IO;
using System.Text;

public class DataFrameManager
{
    private DataTable dt;

    public DataFrameManager()
    {
        // Initialize the DataTable with column names but no rows
        dt = new DataTable();

        dt.Columns.Add("episodeNr", typeof(int));
        dt.Columns.Add("reward", typeof(double));
        dt.Columns.Add("endEvent", typeof(string));
        dt.Columns.Add("velocity", typeof(double));
        dt.Columns.Add("steps", typeof(int));
        dt.Columns.Add("time", typeof(double));



    }

    public void AppendRow(int episodeNr, double reward, string endEvent, double velocity, int steps, double time)
    {
        // Add a new row to the DataTable
        dt.Rows.Add(episodeNr, reward, endEvent, velocity, steps, time);
    }

    public DataTable GetDataTable()
    {
        return dt;
    }

    public void SaveToCsv(string filePath)
    {
        using (StreamWriter sw = new StreamWriter(filePath))
        {
            foreach (DataColumn column in dt.Columns)
            {
                sw.Write($"{column.ColumnName},");
            }

            sw.Write(sw.NewLine);

            foreach (DataRow row in dt.Rows)
            {
                object[] array = row.ItemArray;

                for (int i = 0; i < array.Length - 1; i++)
                {
                    sw.Write($"{array[i].ToString()},");
                }

                sw.Write(array[array.Length - 1].ToString());
                sw.Write(sw.NewLine);
            }

            sw.Close();
        }
    }
}
