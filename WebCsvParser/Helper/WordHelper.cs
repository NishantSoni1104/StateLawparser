using System;
using System.Collections.Generic;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using WebCsvParser.ViewModels;

namespace WebCsvParser.Helper
{
    public class WordHelper
    {
        private readonly string _firstCellWidth;
        private readonly string _secondCellWidth;
        private readonly string _thirdCellWidth;
        private readonly string _fontSize;
        private readonly RunFonts _runFonts;

        public WordHelper(string firstCellWidth, string secondCellWidth, string thirdCellWidth, string fontSize, RunFonts runFonts)
        {
            _firstCellWidth = firstCellWidth;
            _secondCellWidth = secondCellWidth;
            _thirdCellWidth = thirdCellWidth;
            _fontSize = fontSize;
            _runFonts = runFonts;
        }

        /// <summary>
        /// Take the data and build a table at the end of the supplied document.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="lineItems"></param>
        public void AddTable(string fileName, IEnumerable<LineItemViewModel> lineItems)
        {
            using (var document = WordprocessingDocument.Open(fileName, true))
            {

                var doc = document.MainDocumentPart.Document;

                var table = new Table();

                var props = new TableProperties(
                    new TableBorders(
                        new TopBorder
                        {
                            Val = new EnumValue<BorderValues>(BorderValues.Single),
                            Size = 4
                        },
                        new BottomBorder
                        {
                            Val = new EnumValue<BorderValues>(BorderValues.Single),
                            Size = 4
                        },
                        new LeftBorder
                        {
                            Val = new EnumValue<BorderValues>(BorderValues.Single),
                            Size = 4
                        },
                        new RightBorder
                        {
                            Val = new EnumValue<BorderValues>(BorderValues.Single),
                            Size = 4
                        },
                        new InsideHorizontalBorder
                        {
                            Val = new EnumValue<BorderValues>(BorderValues.Single),
                            Size = 4
                        },
                        new InsideVerticalBorder
                        {
                            Val = new EnumValue<BorderValues>(BorderValues.Single),
                            Size = 4
                        }))
                {
                    TableWidth = new TableWidth
                    {
                        Width = "100%"
                    }

                };


                table.AppendChild(props);

                table.AppendChild(CreateRow(
                    $"Item Numbers {Environment.NewLine} (AS IT APPEARS ON QUOTATION)",
                    "Description",
                    "Comments"));

                var category = string.Empty;

                foreach (var lineItem in lineItems)
                {
                    if (string.IsNullOrEmpty(category) || category != lineItem.Category)
                    {
                        table.AppendChild(CreateMergedCellRow(lineItem.Category, isBold: true));
                        category = lineItem.Category;
                    }

                    table.AppendChild(CreateRow(
                        lineItem.WordLineNumbers,
                        lineItem.Name,
                        lineItem.Description));
                }

                doc.Body.AppendChild(table);
                doc.Save();
            }
        }

        /// <summary>
        /// Create 
        /// </summary>
        /// <param name="filepath"></param>
        public static void CreateWordprocessingDocument(string filepath)
        {
            // Create a document by supplying the filepath. 
            using (var wordDocument = WordprocessingDocument.Create(filepath, WordprocessingDocumentType.Document))
            {
                // Add a main document part. 
                var mainPart = wordDocument.AddMainDocumentPart();

                // Create the document structure and add some text.
                mainPart.Document = new Document();
                mainPart.Document.AppendChild(new Body());
            }
        }

        private TableRow CreateMergedCellRow(string firstCellText, bool isBold)
        {
            return CreateRowAsync((firstCell, secondCell, thirdCell) =>
            {
                CreateMergedColumn(firstCell, firstCellText, _firstCellWidth, isBold);

                firstCell.AppendChild(new TableCellProperties
                {
                    HorizontalMerge = new HorizontalMerge
                    {
                        Val = MergedCellValues.Restart
                    }
                });

                CreateMergedColumn(secondCell, "", _secondCellWidth);


                secondCell.AppendChild(new TableCellProperties
                {
                    HorizontalMerge = new HorizontalMerge
                    {
                        Val = MergedCellValues.Continue
                    }
                });

                CreateMergedColumn(thirdCell, "", _thirdCellWidth);

                thirdCell.AppendChild(new TableCellProperties
                {
                    HorizontalMerge = new HorizontalMerge
                    {
                        Val = MergedCellValues.Continue
                    }
                });
            });
        }

        private TableRow CreateRow(string firstCellText, string secondCellText, string thirdCellText)
        {
            return CreateRowAsync((firstCell, secondCell, thirdCell) =>
            {
                var firstRun = new Run();
                ParseTextForOpenXml(firstRun, firstCellText);
                firstCell.AppendChild(new Paragraph(firstRun));

                var secondRun = new Run();
                ParseTextForOpenXml(secondRun, secondCellText);
                secondCell.AppendChild(new Paragraph(secondRun));

                var thirdRun = new Run();
                ParseTextForOpenXml(thirdRun, thirdCellText);
                thirdCell.AppendChild(new Paragraph(thirdRun));
            });

        }

        private TableRow CreateRowAsync(Action<TableCell, TableCell, TableCell> callback)
        {

            var tableRow = new TableRow();

            var firstCell = new TableCell();
            firstCell.AppendChild(new TableCellProperties
            {
                TableCellWidth = new TableCellWidth
                {
                    Width = _firstCellWidth
                }
            });

            tableRow.AppendChild(firstCell);

            var secondCell = new TableCell();
            secondCell.AppendChild(new TableCellProperties
            {
                TableCellWidth = new TableCellWidth
                {
                    Width = _secondCellWidth
                }
            });

            tableRow.AppendChild(secondCell);

            var thirdCell = new TableCell();
            thirdCell.AppendChild(new TableCellProperties
            {
                TableCellWidth = new TableCellWidth
                {
                    Width = _thirdCellWidth
                }
            });

            tableRow.AppendChild(thirdCell);

            callback(firstCell, secondCell, thirdCell);

            return tableRow;
        }

        private void CreateMergedColumn(TableCell tableCell, string text, string width, bool isBold = false)
        {
            var run = new Run();

            if (!string.IsNullOrEmpty(text))
                ParseTextForOpenXml(run, text, isBold);

            tableCell.AppendChild(new Paragraph(run));

            tableCell.AppendChild(new TableCellProperties
            {
                TableCellWidth = new TableCellWidth
                {
                    Width = width
                }
            });
        }

        private void ParseTextForOpenXml(Run run, string textualData, bool isBold = false)
        {
            string[] newLineArray = { Environment.NewLine };
            var textArray = textualData.Split(newLineArray, StringSplitOptions.None);

            var first = true;

            foreach (var line in textArray)
            {
                if (!first)
                {
                    run.AppendChild(new Break());
                }

                first = false;
                
                run.AppendChild(new Text { Text = line });
            }

            var runProperties = new RunProperties();

            if (isBold)
                runProperties.Bold = new Bold();

            runProperties.RunFonts = new RunFonts
            {
                Ascii = _runFonts.Ascii
            };

            runProperties.FontSize = new FontSize
            {
                Val = _fontSize
            };

            run.RunProperties = runProperties;
        }
    }
}
