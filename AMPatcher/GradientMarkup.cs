using Spectre.Console;
using Spectre.Console.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMPatcher
{
    public static class GradientMarkup
    {
        public static string DefaultStartColor { get; set; } = "#DE6262";

        public static string DefaultEndColor { get; set; } = "#FFB88C";

        public static int ColorSteps { get; set; } = 16;

        public static string Text(string str, string startColor = null, string endColor = null)
        {
            startColor = string.IsNullOrEmpty(startColor) ? DefaultStartColor : startColor;
            endColor = string.IsNullOrEmpty(endColor) ? DefaultEndColor : endColor;

            var colors = GetGradients(System.Drawing.ColorTranslator.FromHtml(startColor), System.Drawing.ColorTranslator.FromHtml(endColor), ColorSteps);
            var markup = string.Empty;
            var characterColorWidth = 1.0f / str.Length;
            var colorWidth = 1.0f / ColorSteps;
            for (int i = 0; i < str.Length; i++)
            {
                var colorPos = colors.ElementAt((int)((i * characterColorWidth) / colorWidth));
                markup += $"[{System.Drawing.ColorTranslator.ToHtml(colorPos)}]{str[i]}[/]";
            }

            return markup;
        }

        public static string Ascii(string str, FigletFont fnt, string startColor = null, string endColor = null)
        {
            startColor = string.IsNullOrEmpty(startColor) ? DefaultStartColor : startColor;
            endColor = string.IsNullOrEmpty(endColor) ? DefaultEndColor : endColor;

            var colors = GetGradients(System.Drawing.ColorTranslator.FromHtml(startColor), System.Drawing.ColorTranslator.FromHtml(endColor), ColorSteps);
            var markup = string.Empty;
            var renderSegments = new List<Queue<Segment>>();
            var characterColorWidth = 1.0f / str.Length;
            var colorWidth = 1.0f / ColorSteps;
            for (int i = 0; i < str.Length; i++)
            {
                var colorPos = colors.ElementAt((int)((i * characterColorWidth) / colorWidth));
                var txt = new FigletText(fnt, str[i].ToString()).Color(new Color(colorPos.R, colorPos.G, colorPos.B));
                var segments = txt.GetSegments(AnsiConsole.Console);

                if (!renderSegments.Any())
                {
                    renderSegments.AddRange(Enumerable.Repeat(0, fnt.Height).Select(x => new Queue<Segment>()));
                }

                for (int idx = 0; idx < renderSegments.Count; idx++)
                {
                    renderSegments[idx].Enqueue(segments.ElementAt(2 * idx));
                }
            }

            foreach (var renderSegment in renderSegments)
            {
                foreach (var seg in renderSegment)
                {
                    markup += $"[#{seg.Style.Foreground.ToHex()}]{seg.Text}[/]";
                }
                markup += "\r\n";
            }

            return markup;
        }

        private static IEnumerable<System.Drawing.Color> GetGradients(System.Drawing.Color start, System.Drawing.Color end, int steps)
        {
            int stepA = ((end.A - start.A) / (steps - 1));
            int stepR = ((end.R - start.R) / (steps - 1));
            int stepG = ((end.G - start.G) / (steps - 1));
            int stepB = ((end.B - start.B) / (steps - 1));

            for (int i = 0; i < steps; i++)
            {
                yield return System.Drawing.Color.FromArgb(start.A + (stepA * i),
                                            start.R + (stepR * i),
                                            start.G + (stepG * i),
                                            start.B + (stepB * i));
            }
        }
    }
}
