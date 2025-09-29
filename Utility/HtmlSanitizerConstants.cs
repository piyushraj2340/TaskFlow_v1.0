using Ganss.Xss;
using System.Text.RegularExpressions;

namespace TaskMonitoringApp.Utility
{
    public enum HtmlFeature
    {
        BasicText,
        Links,
        Images,
        Media,
        Iframes,
        Tables,
        Code
    }

    public static class HtmlSanitizerConstants
    {
        public static readonly string[] BasicTextTags = { "p", "br", "span", "div", "strong", "em", "u", "h1", "h2", "h3", "h4", "h5", "h6", "blockquote" };
        public static readonly string[] BasicTextAttrs = { "style" };

        public static readonly string[] LinkTags = { "a" };
        public static readonly string[] LinkAttrs = { "href", "title", "target" };

        public static readonly string[] ImageTags = { "img" };
        public static readonly string[] ImageAttrs = { "src", "alt", "width", "height" };

        public static readonly string[] MediaTags = { "video", "audio", "source" };
        public static readonly string[] MediaAttrs = { "src", "type", "controls", "width", "height" };

        public static readonly string[] IframeTags = { "iframe" };
        public static readonly string[] IframeAttrs = { "src", "frameborder", "allow", "allowfullscreen", "width", "height" };

        public static readonly string[] TableTags = { "table", "thead", "tbody", "tr", "td", "th", "caption", "col", "colgroup" };
        public static readonly string[] TableAttrs = { "style" };

        public static readonly string[] CodeTags = { "pre", "code" };
    }

    public static class HtmlSanitizerHelper
    {
        // Trusted iframe/video sources
        private static readonly string[] TrustedHosts =
        {
        "youtube.com", "youtu.be", "player.vimeo.com"
    };

        public static HtmlSanitizer CreateSanitizer(params HtmlFeature[] features)
        {
            var sanitizer = new HtmlSanitizer();

            foreach (var feature in features)
            {
                switch (feature)
                {
                    case HtmlFeature.BasicText:
                        AddAllowed(sanitizer, HtmlSanitizerConstants.BasicTextTags, HtmlSanitizerConstants.BasicTextAttrs);
                        break;
                    case HtmlFeature.Links:
                        AddAllowed(sanitizer, HtmlSanitizerConstants.LinkTags, HtmlSanitizerConstants.LinkAttrs);
                        break;
                    case HtmlFeature.Images:
                        AddAllowed(sanitizer, HtmlSanitizerConstants.ImageTags, HtmlSanitizerConstants.ImageAttrs);
                        break;
                    case HtmlFeature.Media:
                        AddAllowed(sanitizer, HtmlSanitizerConstants.MediaTags, HtmlSanitizerConstants.MediaAttrs);
                        break;
                    case HtmlFeature.Iframes:
                        AddAllowed(sanitizer, HtmlSanitizerConstants.IframeTags, HtmlSanitizerConstants.IframeAttrs);
                        RestrictIframeSources(sanitizer);
                        break;
                    case HtmlFeature.Tables:
                        AddAllowed(sanitizer, HtmlSanitizerConstants.TableTags, HtmlSanitizerConstants.TableAttrs);
                        break;
                    case HtmlFeature.Code:
                        AddAllowed(sanitizer, HtmlSanitizerConstants.CodeTags, Array.Empty<string>());
                        break;
                }
            }

            return sanitizer;
        }

        private static void AddAllowed(HtmlSanitizer sanitizer, string[] tags, string[] attrs)
        {
            foreach (var tag in tags)
                sanitizer.AllowedTags.Add(tag);

            foreach (var attr in attrs)
                sanitizer.AllowedAttributes.Add(attr);
        }

        private static void RestrictIframeSources(HtmlSanitizer sanitizer)
        {
            sanitizer.RemovingAttribute += (s, e) =>
            {
                if (e.Tag.TagName.Equals("iframe", StringComparison.OrdinalIgnoreCase)
                    && e.Attribute.Name.Equals("src", StringComparison.OrdinalIgnoreCase))
                {
                    var src = e.Attribute.Value ?? "";
                    if (TrustedHosts.Any(host => src.Contains(host, StringComparison.OrdinalIgnoreCase)))
                    {
                        e.Cancel = true;
                    }
                }
            };
        }

        public static string ToPlainText(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return string.Empty;

            // Very basic strip: remove tags
            var noTags = Regex.Replace(html, "<.*?>", string.Empty);

            // Optionally decode HTML entities (&nbsp;, &amp;, etc.)
            return System.Net.WebUtility.HtmlDecode(noTags).Trim();
        }

    }

}
