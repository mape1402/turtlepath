using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace TurtlePath.Studio.App.Guides;

public static partial class SimpleMarkdownRenderer
{
    public static string Render(string markdown, string title)
    {
        var body = RenderBody(markdown ?? string.Empty);
        return $$"""
            <!doctype html>
            <html lang="en">
            <head>
              <meta charset="utf-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1" />
              <title>{{WebUtility.HtmlEncode(title)}}</title>
              <style>
                :root { color-scheme:light; --ink:#081f1a; --muted:#5a7168; --panel:#ffffff; --surface:#f4f8f5; --line:#d9e5de; --primary:#2e7143; --primary-dark:#083229; --soft:#eaf5ee; --code:#1e1e1e; }
                * { box-sizing:border-box; }
                html { scroll-behavior:smooth; }
                body { margin:0; background:var(--surface); color:var(--ink); font:14px/1.62 "Segoe UI", Arial, sans-serif; overflow-x:hidden; }
                .shell { display:grid; grid-template-columns:280px minmax(0, 1fr); min-height:100vh; }
                .sidebar { position:sticky; top:0; height:100vh; overflow:auto; padding:28px 20px; background:#07382e; color:#dcebe5; }
                .brand { display:flex; align-items:center; gap:12px; margin-bottom:24px; }
                .brand-badge { width:42px; height:42px; border-radius:12px; background:#eaf5ee; display:grid; place-items:center; overflow:hidden; color:#2e7143; font-weight:900; }
                .brand h1 { margin:0; font-size:18px; line-height:1.12; color:white; }
                .brand p { margin:2px 0 0; color:#a9c7ba; font-size:12px; }
                .nav-title { margin:18px 0 8px; font-size:11px; text-transform:uppercase; letter-spacing:.08em; color:#9ab9ad; font-weight:700; }
                .nav a { display:block; color:#dcebe5; text-decoration:none; padding:7px 10px; border-radius:8px; margin:1px 0; }
                .nav a:hover, .nav a.active { background:#114d3e; color:white; }
                .nav a.depth-3 { padding-left:24px; font-size:12px; color:#a9c7ba; }
                .content { min-width:0; padding:36px 48px 72px; }
                .article { max-width:1060px; margin:0 auto; background:var(--panel); border:1px solid var(--line); box-shadow:0 10px 34px rgba(8,50,41,.06); padding:44px 54px; }
                h1 { margin:0 0 10px; font-size:38px; line-height:1.08; letter-spacing:0; }
                h2 { margin:44px 0 14px; padding-top:8px; font-size:26px; line-height:1.2; border-top:1px solid var(--line); }
                h3 { margin:28px 0 10px; font-size:20px; line-height:1.25; }
                h4, h5, h6 { margin:24px 0 8px; }
                p { margin:0 0 14px; color:var(--muted); }
                ul, ol { margin:0 0 16px 22px; padding:0; }
                li { margin:6px 0; }
                code { font-family:Consolas, "Cascadia Mono", monospace; background:#f3f3f3; color:#001080; padding:2px 5px; border-radius:5px; font-size:.92em; }
                .code-card { margin:18px 0 24px; border:1px solid #3c3c3c; background:var(--code); overflow:hidden; box-shadow:0 12px 26px rgba(0,0,0,.12); }
                .code-toolbar { display:flex; align-items:center; justify-content:space-between; gap:16px; padding:9px 12px; background:#0d2a25; color:#b8d6c8; border-bottom:1px solid #173d35; font-size:12px; font-weight:700; }
                .code-toolbar span { color:#b8d6c8; text-transform:uppercase; letter-spacing:.04em; }
                .copy-code { border:1px solid #315d50; background:#123d34; color:white; border-radius:4px; padding:6px 10px; font:700 12px "Segoe UI", Arial, sans-serif; cursor:pointer; }
                .copy-code:hover { background:#2e7143; border-color:#2e7143; }
                .copy-code.copied { background:#7ccc55; border-color:#7ccc55; color:#062019; }
                pre { margin:0; padding:18px 20px; overflow:auto; }
                pre code { display:block; background:transparent; color:#d4d4d4; padding:0; border-radius:0; line-height:1.52; font-size:13px; tab-size:4; white-space:pre; }
                a { color:var(--primary); }
                .article > h1:first-child + p { font-size:17px; color:#36554b; margin-bottom:28px; }
                blockquote { border-left:4px solid var(--primary); margin:16px 0; padding:6px 0 6px 14px; color:var(--muted); background:#f7faf6; }
                @media (max-width:980px) {
                  .shell { grid-template-columns:1fr; }
                  .sidebar { position:relative; height:auto; }
                  .content { padding:18px; }
                  .article { padding:28px 24px; }
                }
              </style>
            </head>
            <body>
              <div class="shell">
                <aside class="sidebar">
                  <div class="brand"><div class="brand-badge">TP</div><div><h1>TurtlePath<br>Template</h1><p>Use guide</p></div></div>
                  <div class="nav-title">Guide sections</div>
                  <nav class="nav">{{BuildNav(body)}}</nav>
                </aside>
                <main class="content"><article class="article">{{body}}</article></main>
              </div>
              <script>
                document.querySelectorAll('a[href^="#"]').forEach(link => {
                  link.addEventListener('click', event => {
                    event.preventDefault();
                    const id = decodeURIComponent(link.getAttribute('href').slice(1));
                    const target = document.getElementById(id);
                    if (!target) return;

                    document.querySelectorAll('.nav a').forEach(item => item.classList.remove('active'));
                    const navLink = document.querySelector(`.nav a[href="#${CSS.escape(id)}"]`);
                    if (navLink) navLink.classList.add('active');
                    target.scrollIntoView({ behavior: 'smooth', block: 'start' });
                    history.replaceState(null, '', '#' + encodeURIComponent(id));
                  });
                });

                document.querySelectorAll('.copy-code').forEach(button => {
                  button.addEventListener('click', async () => {
                    const code = button.closest('.code-card').querySelector('code').innerText;
                    await navigator.clipboard.writeText(code);
                    const old = button.innerText;
                    button.innerText = 'Copied';
                    button.classList.add('copied');
                    setTimeout(() => { button.innerText = old; button.classList.remove('copied'); }, 1000);
                  });
                });
              </script>
            </body>
            </html>
            """;
    }

    private static string RenderBody(string markdown)
    {
        var html = new StringBuilder();
        var inCode = false;
        var code = new StringBuilder();
        var codeLanguage = "text";
        var inList = false;

        foreach (var rawLine in markdown.Replace("\r\n", "\n").Split('\n'))
        {
            var line = rawLine.TrimEnd();
            if (line.StartsWith("```", StringComparison.Ordinal))
            {
                if (inCode)
                {
                    html.AppendLine(RenderCode(codeLanguage, code.ToString()));
                    code.Clear();
                    codeLanguage = "text";
                    inCode = false;
                }
                else
                {
                    CloseList(html, ref inList);
                    codeLanguage = line.Length > 3 ? line[3..].Trim() : "text";
                    if (string.IsNullOrWhiteSpace(codeLanguage))
                        codeLanguage = "text";
                    inCode = true;
                }

                continue;
            }

            if (inCode)
            {
                code.AppendLine(line);
                continue;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                CloseList(html, ref inList);
                continue;
            }

            if (line.StartsWith("# ", StringComparison.Ordinal))
            {
                CloseList(html, ref inList);
                html.AppendLine(Heading(1, line[2..]));
                continue;
            }

            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                CloseList(html, ref inList);
                html.AppendLine(Heading(2, line[3..]));
                continue;
            }

            if (line.StartsWith("### ", StringComparison.Ordinal))
            {
                CloseList(html, ref inList);
                html.AppendLine(Heading(3, line[4..]));
                continue;
            }

            if (line.StartsWith("- ", StringComparison.Ordinal))
            {
                if (!inList)
                {
                    html.AppendLine("<ul>");
                    inList = true;
                }

                html.AppendLine($"<li>{Inline(line[2..])}</li>");
                continue;
            }

            CloseList(html, ref inList);
            html.AppendLine($"<p>{Inline(line)}</p>");
        }

        CloseList(html, ref inList);

        if (inCode)
            html.AppendLine(RenderCode(codeLanguage, code.ToString()));

        return html.ToString();
    }

    private static string Heading(int level, string text)
    {
        var encoded = Inline(text);
        var id = SlugRegex().Replace(WebUtility.HtmlDecode(encoded).ToLowerInvariant(), "-").Trim('-');
        return $"<h{level} id=\"{id}\">{encoded}</h{level}>";
    }

    private static string Inline(string text)
    {
        var encoded = WebUtility.HtmlEncode(text);
        encoded = CodeRegex().Replace(encoded, match => $"<code>{match.Groups[1].Value}</code>");
        encoded = LinkRegex().Replace(encoded, match => $"<a href=\"{match.Groups[2].Value}\">{match.Groups[1].Value}</a>");
        encoded = BoldRegex().Replace(encoded, "<strong>$1</strong>");
        return encoded;
    }

    private static string RenderCode(string language, string text)
    {
        return $$"""
            <div class="code-card">
              <div class="code-toolbar"><span>{{WebUtility.HtmlEncode(language)}}</span><button type="button" class="copy-code">Copy</button></div>
              <pre><code class="language-{{WebUtility.HtmlEncode(language)}}">{{WebUtility.HtmlEncode(text.TrimEnd())}}</code></pre>
            </div>
            """;
    }

    private static string BuildNav(string body)
    {
        return HeadingRegex().Matches(body)
            .Select(match => new
            {
                Level = int.Parse(match.Groups[1].Value),
                Id = match.Groups[2].Value,
                Text = match.Groups[3].Value
            })
            .Aggregate(new StringBuilder(), (builder, heading) =>
            {
                if (heading.Level is 2 or 3)
                    builder.AppendLine($"<a class=\"depth-{heading.Level}\" href=\"#{heading.Id}\">{heading.Text}</a>");

                return builder;
            })
            .ToString();
    }

    private static void CloseList(StringBuilder html, ref bool inList)
    {
        if (!inList)
            return;

        html.AppendLine("</ul>");
        inList = false;
    }

    [GeneratedRegex("<h([1-3]) id=\"([^\"]+)\">(.+?)</h\\1>")]
    private static partial Regex HeadingRegex();

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex SlugRegex();

    [GeneratedRegex("`([^`]+)`")]
    private static partial Regex CodeRegex();

    [GeneratedRegex("\\[([^\\]]+)\\]\\(([^)]+)\\)")]
    private static partial Regex LinkRegex();

    [GeneratedRegex("\\*\\*([^*]+)\\*\\*")]
    private static partial Regex BoldRegex();
}
