﻿using BaconographyPortable.Services;
using BaconographyPortable.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Markup;
using SnuDom;
using Windows.UI.Xaml.Media;
using Windows.UI.Text;
using BaconographyW8.View.Markdown;
using Windows.UI;

namespace BaconographyW8.Converters
{
    public class MarkdownConverter : IValueConverter
    {
        unsafe public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is MarkdownData)
            {
                var markdownData = value as MarkdownData;
                try
                {
                    var categoryVisitor = new SnuDomCategoryVisitor();
                    ((IDomObject)markdownData.MarkdownDom).Accept(categoryVisitor);
                    switch (categoryVisitor.Category)
                    {
                        case MarkdownCategory.PlainText:
                            {
                                var visitor = new SnuDomPlainTextVisitor();
                                ((IDomObject)markdownData.MarkdownDom).Accept(visitor);
                                return MakePlain(visitor.Result);
                            }
                        case MarkdownCategory.Formatted:
                        case MarkdownCategory.Full:
                            {
                                var visitor = new SnuDomFullUIVisitor(new SolidColorBrush(Colors.White));
                                ((IDomObject)markdownData.MarkdownDom).Accept(visitor);
                                if (visitor.ResultGroup != null)
                                    return visitor.ResultGroup;
                                else
                                    return visitor.Result;
                            }
                        default:
                            return new TextBlock { Text = "" };
                    }

                }
                catch (Exception ex)
                {
                    return MakePlain(ex.ToString());
                }
            }
            else
                return new TextBlock { Text = "" };
        }

        private object MakePlain(string value)
        {
            return new TextBlock { Text = value as string, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 0) };
        }


        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    class SnuDomFullUIVisitor : IDomVisitor
    {
        public SnuDomFullUIVisitor(Brush forgroundBrush)
        {
            _forgroundBrush = forgroundBrush;
        }
        Brush _forgroundBrush;
        private int _textLengthInCurrent = 0;
        public RichTextBlock Result = new RichTextBlock { TextWrapping = TextWrapping.Wrap, FontFamily = new FontFamily("Segoe UI"), FontSize = 15, FontStyle = FontStyle.Normal, FontWeight = FontWeights.Normal };
        public StackPanel ResultGroup = null;
        Windows.UI.Xaml.Documents.Paragraph _currentParagraph;

        private void MaybeSplitForParagraph()
        {
            if (_textLengthInCurrent > 1000)
            {
                if (ResultGroup == null)
                {
                    ResultGroup = new StackPanel { Orientation = Orientation.Vertical };
                    ResultGroup.Children.Add(Result);

                }

				ResultGroup.Children.Add(Result = new RichTextBlock { TextWrapping = TextWrapping.Wrap, FontFamily = new FontFamily("Segoe UI"), FontSize = 15 });
                _textLengthInCurrent = 0;
            }

			if (_currentParagraph != null)
			{
				_currentParagraph.Inlines.Add(new Windows.UI.Xaml.Documents.LineBreak());
			}

			_currentParagraph = new Windows.UI.Xaml.Documents.Paragraph { TextAlignment = TextAlignment.Left };
			Result.Blocks.Add(_currentParagraph);
        }

		private void DirectlyPlaceUIContent(UIElement element)
		{
			if (ResultGroup == null)
			{
				ResultGroup = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(0) };
				if (Result.Blocks.Count == 0)
				{
					//nothing here yet so lets just ignore the current result and move on
				}
				else
				{
					ResultGroup.Children.Add(Result);
				}
			}
			else if (ResultGroup.Children.Last() is RichTextBlock && ((RichTextBlock)ResultGroup.Children.Last()).Blocks.Count == 0)
			{
				ResultGroup.Children.Remove(ResultGroup.Children.Last());
			}
			ResultGroup.Children.Add(element);

			ResultGroup.Children.Add(Result = new RichTextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, -2, 0, 3), FontFamily = new FontFamily("Segoe UI"), FontSize = 15 });
			_textLengthInCurrent = 0;
		}

        public void Visit(Text text)
        {
            var madeRun = new Run { Text = text.Contents };
            _textLengthInCurrent += text.Contents.Length;

            if (text.Italic)
                madeRun.FontStyle = Windows.UI.Text.FontStyle.Italic;

            if (text.Bold)
                madeRun.FontWeight = FontWeights.Bold;


            if (text.HeaderSize != 0)
            {
                switch (text.HeaderSize)
                {
                    case 1:
                        madeRun.FontSize = 15;
                        break;
                    case 2:
                        madeRun.FontSize = 16;
                        madeRun.FontWeight = FontWeights.Bold;
                        madeRun.Foreground = _forgroundBrush;
                        break;
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                        madeRun.FontSize = 18;
                        madeRun.FontWeight = FontWeights.Bold;
                        break;
                }
                MaybeSplitForParagraph();
                _currentParagraph.Inlines.Add(madeRun);
                if (text.HeaderSize == 1)
                {
                    var inlineContainer = new InlineUIContainer();
                    inlineContainer.Child = new Border
                    {
                        Margin = new Thickness(0, 3, 0, 3),
                        Height = 1,
                        VerticalAlignment = VerticalAlignment.Top,
                        BorderBrush = _forgroundBrush,
                        BorderThickness = new Thickness(1),
                        MinWidth = 1800
                    };
                    _currentParagraph.Inlines.Add(inlineContainer);
                }
                else
                    _currentParagraph.Inlines.Add(new Windows.UI.Xaml.Documents.LineBreak());

            }
            else
            {
                if (_currentParagraph == null)
                {
					_currentParagraph = new Windows.UI.Xaml.Documents.Paragraph { TextAlignment = TextAlignment.Left };
                    Result.Blocks.Add(_currentParagraph);
                }
                _currentParagraph.Inlines.Add(madeRun);
            }
        }

        public void Visit(SnuDom.Paragraph paragraph)
        {
            MaybeSplitForParagraph();
            foreach (var elem in paragraph)
            {
                elem.Accept(this);
            }

        }

        public void Visit(HorizontalRule horizontalRule)
        {
            var inlineContainer = new InlineUIContainer();
            inlineContainer.Child = new Border
            {
                Margin = new Thickness(3, 3, 0, 3),
                Height = 2,
                VerticalAlignment = VerticalAlignment.Top,
                BorderBrush = _forgroundBrush,
                BorderThickness = new Thickness(2),
                MinWidth = 1800
            };
            MaybeSplitForParagraph();
            _currentParagraph.Inlines.Add(inlineContainer);
        }

        public void Visit(SnuDom.LineBreak lineBreak)
        {
            _currentParagraph.Inlines.Add(new Windows.UI.Xaml.Documents.LineBreak());
        }

        public void Visit(Link link)
        {
			if (link.Display.Count() == 0 &&
				(link.Url.StartsWith("#") || link.Url.StartsWith("/#") ||
				link.Url.StartsWith("//#") || (link.Url.StartsWith("/") && link.Url.ToCharArray().Count(ch => ch == '/') == 1)))
			{
				return;
			}

			var inlineContainer = new InlineUIContainer { FontSize = 15 };
            SnuDomCategoryVisitor categoryVisitor = new SnuDomCategoryVisitor();
            if (link.Display != null)
            {
                foreach (var item in link.Display)
                {
                    item.Accept(categoryVisitor);
                }
            }

            if (categoryVisitor.Category == MarkdownCategory.PlainText)
            {
                var plainTextVisitor = new SnuDomPlainTextVisitor();
				if (link.Display != null && link.Display.FirstOrDefault() != null)
                {
                    foreach (var item in link.Display)
                        item.Accept(plainTextVisitor);
                }
                else
                    plainTextVisitor.Result = link.Url;

                inlineContainer.Child = new MarkdownLink(link.Url, plainTextVisitor.Result);
				//((Hyperlink)inlineContainer).Inlines.Add(plainTextVisitor.Result);
				//inlineContainer.Child = new MarkdownButton(link.Url, plainTextVisitor.Result);
            }
            else
            {
				//inlineContainer = new Hyperlink { Command = new RelayCommand<string>(UtilityCommandImpl.GotoLinkImpl), CommandParameter = link.Url };
				var text = link.Display.FirstOrDefault() as Text;
				if (text != null)
				{
					if (text.Italic)
						inlineContainer.FontStyle = FontStyle.Italic;

					if (text.Bold)
						inlineContainer.FontWeight = FontWeights.Bold;


					if (text.HeaderSize != 0)
					{
						switch (text.HeaderSize)
						{
							case 1:
								inlineContainer.FontSize = 15;
								break;
							case 2:
								inlineContainer.FontSize = 16;
								inlineContainer.FontWeight = FontWeights.Bold;
								inlineContainer.Foreground = _forgroundBrush;
								break;
							case 3:
							case 4:
							case 5:
							case 6:
								inlineContainer.FontSize = 18;
								inlineContainer.FontWeight = FontWeights.Bold;
								break;
						}
					}

					var plainTextVisitor = new SnuDomPlainTextVisitor();
					if (link.Display != null && link.Display.FirstOrDefault() != null)
					{
						foreach (var item in link.Display)
							item.Accept(plainTextVisitor);
					}
					else
						plainTextVisitor.Result = link.Url;

					inlineContainer.Child = new MarkdownLink(link.Url, plainTextVisitor.Result);
				}
				else
				{
					inlineContainer = new InlineUIContainer();
					var fullUIVisitor = new SnuDomFullUIVisitor(_forgroundBrush);
					//cant be null in this category
					foreach (var item in link.Display)
						item.Accept(fullUIVisitor);

					inlineContainer.Child = new MarkdownLink(link.Url, fullUIVisitor.Result);
				}
			}

			if (_currentParagraph == null)
			{
				MaybeSplitForParagraph();
			}

			_currentParagraph.Inlines.Add(inlineContainer);
        }

        public void Visit(Code code)
        {
            var plainTextVisitor = new SnuDomPlainTextVisitor();

            foreach (var item in code)
                item.Accept(plainTextVisitor);

            var madeRun = new Run { Text = plainTextVisitor.Result };
            if (_currentParagraph == null || code.IsBlock)
            {
                MaybeSplitForParagraph();
            }
            _currentParagraph.Inlines.Add(madeRun);
        }

        public void Visit(Quote code)
        {
            SnuDomCategoryVisitor categoryVisitor = new SnuDomCategoryVisitor();
			UIElement result = null;
            foreach (var item in code)
            {
                item.Accept(categoryVisitor);
            }


            if (categoryVisitor.Category == MarkdownCategory.PlainText && code.Count() == 1)
            {
                var plainTextVisitor = new SnuDomPlainTextVisitor();

                foreach (var item in code)
                    item.Accept(plainTextVisitor);


				result = new MarkdownQuote(plainTextVisitor.Result);
            }
            else
            {
                var fullUIVisitor = new SnuDomFullUIVisitor(_forgroundBrush);
                //cant be null in this category
                foreach (var item in code)
                    item.Accept(fullUIVisitor);

				if (fullUIVisitor.ResultGroup != null)
				{
					result = new MarkdownQuote(fullUIVisitor.ResultGroup);
				}
				else
				{
					result = new MarkdownQuote(fullUIVisitor.Result);
				}

            }

            DirectlyPlaceUIContent(result);
        }

		private void FlattenVisitParagraph(IDomVisitor visitor, SnuDom.Paragraph paragraph)
		{
			foreach (var item in paragraph)
			{
				if (item is SnuDom.Paragraph)
				{
					FlattenVisitParagraph(visitor, item as SnuDom.Paragraph);
				}
				else
					item.Accept(visitor);
			}
		}

        private IEnumerable<UIElement> BuildChildUIList(IEnumerable<IDomObject> objects)
        {
			List<UIElement> results = new List<UIElement>();
			foreach (var item in objects)
			{
				SnuDomCategoryVisitor categoryVisitor = new SnuDomCategoryVisitor();


				if (item is TableColumn)
				{
					foreach (var contents in ((TableColumn)item).Contents)
					{
						contents.Accept(categoryVisitor);
					}
				}
				else
				{
					item.Accept(categoryVisitor);
				}


				var column = item as TableColumn;
				IDomObject columnFirstContent = null;

				if (categoryVisitor.Category == MarkdownCategory.PlainText)
				{
					var plainTextVisitor = new SnuDomPlainTextVisitor();
					//this might be a pp
					if (column != null)
					{
						foreach (var contents in column.Contents)
						{
							contents.Accept(plainTextVisitor);
						}
					}
					else if (item is SnuDom.Paragraph)
					{
						item.Accept(plainTextVisitor);
					}

					results.Add(new TextBlock { TextWrapping = TextWrapping.Wrap, FontSize = 15, Text = plainTextVisitor.Result, Margin = new Thickness(0, 3, 0, 3) });
				}
				else if (column != null && ((TableColumn)item).Contents.Count() == 1 && (columnFirstContent = ((TableColumn)item).Contents.FirstOrDefault()) != null &&
					(columnFirstContent is Text))
				{
					if (columnFirstContent is Link)
					{
						var plainTextVisitor = new SnuDomPlainTextVisitor();
						var lnk = columnFirstContent as Link;
						var firstContent = lnk.Display.FirstOrDefault();
						if (firstContent != null)
							firstContent.Accept(plainTextVisitor);
						results.Add(new MarkdownLink(lnk.Url, plainTextVisitor.Result));
					}
					else
					{
						results.Add(new TextBlock { TextWrapping = TextWrapping.Wrap, Text = ((Text)columnFirstContent).Contents, Margin = new Thickness(0, 3, 0, 3) });
					}
				}
				else
				{
					var fullUIVisitor = new SnuDomFullUIVisitor(_forgroundBrush);
					if (column != null)
					{
						foreach (var contents in column.Contents)
						{
							contents.Accept(fullUIVisitor);
						}
					}
					else if (item is SnuDom.Paragraph)
					{
						FlattenVisitParagraph(fullUIVisitor, item as SnuDom.Paragraph);
					}

					if (fullUIVisitor.ResultGroup != null)
						results.Add(fullUIVisitor.ResultGroup);
					else
						results.Add(fullUIVisitor.Result);
				}

				if (column != null)
				{
					switch (column.Alignment)
					{
						case ColumnAlignment.Center:
							results.Last().SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
							break;
						case ColumnAlignment.Left:
							results.Last().SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Left);
							break;
						case ColumnAlignment.Right:
							results.Last().SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Right);
							break;
					}

					results.Last().SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Top);
				}
			}
			return results;
        }

        public void Visit(OrderedList orderedList)
        {
            var uiElements = BuildChildUIList(orderedList);
			DirectlyPlaceUIContent(new MarkdownList(false, uiElements));
        }

        public void Visit(UnorderedList unorderedList)
        {
            var uiElements = BuildChildUIList(unorderedList);
			DirectlyPlaceUIContent(new MarkdownList(false, uiElements));
        }

        public void Visit(Table table)
        {
			var headerUIElements = BuildChildUIList(table.Headers);
			List<IEnumerable<UIElement>> tableBody = new List<IEnumerable<UIElement>>();
			foreach (var row in table.Rows)
			{
				tableBody.Add(BuildChildUIList(row.Columns));
			}

			DirectlyPlaceUIContent(new MarkdownTable(headerUIElements, tableBody));
        }

        public void Visit(Document document)
        {
            foreach (var elem in document)
            {
                elem.Accept(this);
            }
        }

        public void Visit(TableRow tableRow)
        {
            throw new NotImplementedException();
        }

        public void Visit(TableColumn tableColumn)
        {
            foreach (var elem in tableColumn.Contents)
            {
                elem.Accept(this);
            }
        }
    }
}
