using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Integral.Core;
using Xunit;

namespace Integral.Tests
{
    public class ReportTests
    {
        private static ReportInput Input(string f, double a, double b, MethodChoice choice, int n = 100, double eps = 0)
        {
            Node node = Parser.Parse(f);
            var p = new IntegrationParams { A = a, B = b, N = n, Eps = eps };
            var results = new List<IntegrationResult>();
            if (choice != MethodChoice.Simpson) results.Add(Integrator.Integrate(node, p, Method.Trapezoid));
            if (choice != MethodChoice.Trapezoid) results.Add(Integrator.Integrate(node, p, Method.Simpson));
            return new ReportInput { Formula = f, A = a, B = b, N = n, Eps = eps, Method = choice, Function = node, Results = results,
                                     Created = new DateTime(2026, 10, 19, 9, 40, 0) };
        }

        [Fact]
        public void PlotModel_RangesIncludeZero_AndLimits()
        {
            PlotModel m = PlotModel.Build(Parser.Parse("x^2 + 1"), 1, 3, 4, 200, 6, 5);
            Assert.True(m.XMin < 1 && m.XMax > 3);
            Assert.True(m.YMin < 0 && m.YMax > 10);
            Assert.Equal(5, m.Nodes.Count);
            Assert.All(m.XTicks, v => Assert.InRange(v, m.XMin, m.XMax));
        }

        [Fact]
        public void PlotModel_ReversedLimits_AreOrdered()
        {
            PlotModel m = PlotModel.Build(Parser.Parse("x"), 5, 2, 10, 50, 5, 5);
            Assert.Equal(2, m.A);
            Assert.Equal(5, m.B);
        }

        [Fact]
        public void PlotModel_UndefinedPoints_AreGaps()
        {
            PlotModel m = PlotModel.Build(Parser.Parse("ln(x)"), -1, 1, 100, 100, 5, 5);
            Assert.Contains(m.Curve, p => !p.Y.HasValue);
            Assert.Contains(m.Curve, p => p.Y.HasValue);
        }

        [Fact]
        public void PlotModel_Pole_DoesNotFlattenGraph()
        {
            PlotModel m = PlotModel.Build(Parser.Parse("1/(x-0.5001)"), 0, 1, 100, 400, 5, 5);
            Assert.True(m.YMax - m.YMin < 1000, "диапазон y должен отсекать выброс у полюса");
        }

        [Fact]
        public void PlotModel_NoNodes_ForLargeN()
        {
            Assert.Empty(PlotModel.Build(Parser.Parse("x"), 0, 1, 1000, 50, 5, 5).Nodes);
        }

        [Fact]
        public void Svg_IsWellFormedXml()
        {
            PlotModel m = PlotModel.Build(Parser.Parse("sin(x)"), 0, Math.PI, 8, 300, 6, 5);
            XDocument doc = XDocument.Parse(SvgPlot.Render(m, 800, 360, Method.Trapezoid));
            Assert.Equal("svg", doc.Root!.Name.LocalName);
            Assert.True(doc.Descendants().Count(e => e.Name.LocalName == "polyline") >= 2);
            Assert.Contains(doc.Descendants(), e => e.Name.LocalName == "polygon");
        }

        [Fact]
        public void Format_CommaAndPowers()
        {
            Assert.Equal("41,80691784", HtmlFormat.Num(41.806917842, 10));
            Assert.Equal("−0,5", HtmlFormat.Num(-0.5));
            Assert.Equal("5,73·10<sup>−7</sup>", HtmlFormat.Sci(5.73e-7));
            Assert.Equal("1 048 576", HtmlFormat.Int(1048576));
            Assert.Equal("—", HtmlFormat.Num(double.NaN));
        }

        [Fact]
        public void Report_HasAllSections()
        {
            string html = HtmlReport.Build(Input("20*sin(sqrt(x)*3)", 0, 10, MethodChoice.Both, 100, 1e-6));
            foreach (string part in new[] { "<!DOCTYPE html>", "charset=\"utf-8\"", "Исходные данные", "Результаты", "<svg",
                                            "Таблица сходимости — метод трапеций", "Таблица сходимости — метод Симпсона",
                                            "Значения функции", "Метод трапеций", "Метод Симпсона", "19.10.2026" })
                Assert.Contains(part, html);
            Assert.Contains("негладкая", html);
        }

        [Fact]
        public void Report_FixedN_HasNoConvergenceTable()
        {
            string html = HtmlReport.Build(Input("x^2", 0, 1, MethodChoice.Simpson, 10));
            Assert.DoesNotContain("Таблица сходимости", html);
            Assert.Contains("не задана", html);
            Assert.Contains("0,333333333333", html);
        }

        [Fact]
        public void Report_EscapesFormula()
        {
            ReportInput r = Input("x", 0, 1, MethodChoice.Simpson, 2);
            r.Formula = "<b>x</b>";
            string html = HtmlReport.Build(r);
            Assert.DoesNotContain("<b>x</b>", html);
            Assert.Contains("&lt;b&gt;x&lt;/b&gt;", html);
        }

        [Fact]
        public void Report_UndefinedValues_AreMarked()
        {
            string html = HtmlReport.Build(Input("sqrt(x)", 0, 1, MethodChoice.Trapezoid, 10));
            Assert.Contains("Значения функции", html);
            Assert.DoesNotContain("не определена", html);
        }

        [Fact]
        public void Report_RequiresResults()
        {
            Assert.Throws<ArgumentException>(() => HtmlReport.Build(new ReportInput { Formula = "x" }));
        }
    }
}
