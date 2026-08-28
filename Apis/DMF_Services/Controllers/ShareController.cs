using System.Text.Json;
using DMF_Services.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DMF_Services.Controllers
{
    // -------------------------------------------------------------------------
    // Server-rendered car SHARE page (rich link previews).
    //
    // WHY: WhatsApp / Facebook / X crawlers read Open Graph tags from the HTML
    // <head> and DO NOT run JavaScript, so a static page can only ever show one
    // fixed preview. This endpoint fetches the specific car and returns HTML
    // whose <head> already carries THAT car's photo, name and price — giving a
    // per-car rich preview. Humans who open it are auto-forwarded to the app
    // (or the Play Store).
    //
    // The Netlify share site proxies /car/{id} and /?id={id} to this endpoint,
    // so existing share links keep working and the app needs no change.
    //
    // Routes (root-level, NOT under /api/{version}, and no auth):
    //   GET /share/{id}
    //   GET /share?id={id}
    // -------------------------------------------------------------------------
    [ApiController]
    public class ShareController : ControllerBase
    {
        private readonly ICarService _service;

        // Public-facing share site (used for canonical og:url + static assets).
        private const string Site = "https://luminous-boba-ecb3c9.netlify.app";
        private const string PlayUrl = "https://play.google.com/store/apps/details?id=com.dmf.services";
        private const string AppScheme = "dmfmotors"; // technical deep-link scheme baked into the app — do NOT rename
        private const string FallbackImg = Site + "/og-image.png";

        public ShareController(ICarService service)
        {
            _service = service;
        }

        [HttpGet("/share/{id:int}")]
        public Task<ContentResult> ByPath(int id) => Render(id);

        [HttpGet("/share")]
        public Task<ContentResult> ByQuery([FromQuery] int id) => Render(id);

        private async Task<ContentResult> Render(int id)
        {
            string title = "Car Listing";
            string priceText = "";
            string image = FallbackImg;
            var specs = new List<string>();

            try
            {
                var car = await _service.GetByIdAsync(id);
                if (car != null)
                {
                    title = string.Join(" ", new[] { car.Brand, car.Model, car.Varient }
                        .Where(s => !string.IsNullOrWhiteSpace(s))).Trim();
                    if (string.IsNullOrWhiteSpace(title)) title = "Car Listing";

                    if (car.Price.HasValue && car.Price.Value > 0)
                        priceText = "₹" + Inr(car.Price.Value);

                    if (car.RegistrationDate.HasValue)
                        specs.Add(car.RegistrationDate.Value.Year.ToString());
                    if (car.KMDriven.HasValue && car.KMDriven.Value > 0)
                        specs.Add(Inr(car.KMDriven.Value) + " km");
                    if (!string.IsNullOrWhiteSpace(car.Fuel)) specs.Add(car.Fuel!);
                    if (!string.IsNullOrWhiteSpace(car.Transmission)) specs.Add(car.Transmission!);

                    var first = car.Images?.Images?.FirstOrDefault(u => !string.IsNullOrWhiteSpace(u));
                    if (!string.IsNullOrWhiteSpace(first)) image = first!;
                }
            }
            catch
            {
                // DB cold-start / not found -> generic preview below.
            }

            var ogTitle = !string.IsNullOrEmpty(priceText) ? $"{title} — {priceText}" : $"{title} | CarDeals";
            var ogDesc = specs.Count > 0
                ? string.Join("  ·  ", specs)
                : "Check out this car on CarDeals. Tap to open in the app.";
            var appLink = $"{AppScheme}://car/{id}";
            var priceBlock = string.IsNullOrEmpty(priceText) ? "" : $"<div class=\"price\">{E(priceText)}</div>";
            var chips = specs.Count == 0 ? "" :
                "<div class=\"chips\">" + string.Concat(specs.Select(s => $"<span class=\"chip\">{E(s)}</span>")) + "</div>";

            var html = Template
                .Replace("%%PAGE_TITLE%%", E(title))
                .Replace("%%OG_TITLE%%", E(ogTitle))
                .Replace("%%OG_DESC%%", E(ogDesc))
                .Replace("%%OG_URL%%", $"{Site}/car/{id}")
                .Replace("%%IMAGE%%", E(image))
                .Replace("%%TITLE%%", E(title))
                .Replace("%%PRICE_BLOCK%%", priceBlock)
                .Replace("%%CHIPS%%", chips)
                .Replace("%%APP_LINK%%", E(appLink))
                .Replace("%%APP_LINK_JSON%%", JsonSerializer.Serialize(appLink))
                .Replace("%%PLAY_URL%%", PlayUrl)
                .Replace("%%FAVICON%%", Site + "/favicon.png");

            return new ContentResult
            {
                Content = html,
                ContentType = "text/html; charset=utf-8",
                StatusCode = 200
            };
        }

        // Indian digit grouping: 500000 -> "5,00,000"
        private static string Inr(long n)
        {
            var s = n.ToString();
            if (s.Length <= 3) return s;
            var last3 = s.Substring(s.Length - 3);
            var rest = s.Substring(0, s.Length - 3);
            return System.Text.RegularExpressions.Regex.Replace(rest, @"\B(?=(\d{2})+(?!\d))", ",") + "," + last3;
        }

        private static string E(string? s) => System.Net.WebUtility.HtmlEncode(s ?? "");

        // Non-interpolated raw literal: braces in CSS/JS stay literal; %%TOKENS%% are replaced above.
        private const string Template = """
<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="UTF-8" />
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>%%PAGE_TITLE%% | CarDeals</title>
<link rel="icon" type="image/png" href="%%FAVICON%%" />
<meta property="og:site_name" content="CarDeals" />
<meta property="og:title" content="%%OG_TITLE%%" />
<meta property="og:description" content="%%OG_DESC%%" />
<meta property="og:type" content="website" />
<meta property="og:url" content="%%OG_URL%%" />
<meta property="og:image" content="%%IMAGE%%" />
<meta property="og:image:alt" content="%%TITLE%%" />
<meta name="twitter:card" content="summary_large_image" />
<meta name="twitter:title" content="%%OG_TITLE%%" />
<meta name="twitter:description" content="%%OG_DESC%%" />
<meta name="twitter:image" content="%%IMAGE%%" />
<meta name="theme-color" content="#0a0a0c" />
<link rel="preconnect" href="https://fonts.googleapis.com" />
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin />
<link href="https://fonts.googleapis.com/css2?family=Poppins:wght@400;500;600;700;800&display=swap" rel="stylesheet" />
<style>
    :root{--bg:#0a0a0c;--card:#181a22;--border:rgba(255,255,255,.08);--red:#ca2f49;--red2:#e0405b;--text:#fff;--muted:#9a9daa;}
    *{box-sizing:border-box;margin:0;padding:0;}
    html,body{height:100%;background:var(--bg);color:var(--text);font-family:"Poppins",-apple-system,BlinkMacSystemFont,"Segoe UI",Roboto,sans-serif;-webkit-font-smoothing:antialiased;}
    body{display:flex;align-items:center;justify-content:center;padding:20px;min-height:100vh;}
    .card{width:100%;max-width:420px;background:var(--card);border:1px solid var(--border);border-radius:24px;overflow:hidden;box-shadow:0 30px 80px rgba(0,0,0,.55);}
    .hero{position:relative;width:100%;aspect-ratio:4/3;overflow:hidden;background:#0d0f16;}
    .hero .bg{position:absolute;inset:0;background-size:cover;background-position:center;filter:blur(30px) brightness(.5);transform:scale(1.2);}
    .hero img{position:relative;z-index:1;width:100%;height:100%;object-fit:contain;display:block;}
    .brandbar{position:absolute;z-index:2;top:12px;left:12px;display:flex;align-items:center;gap:7px;padding:6px 11px 6px 8px;background:rgba(10,10,12,.6);backdrop-filter:blur(8px);border-radius:999px;font-size:13px;font-weight:700;}
    .brandbar img{width:20px;height:20px;}
    .brandbar .c{color:var(--red);}.brandbar .d{color:#fff;}
    .body{padding:20px 22px 24px;}
    h1{font-size:20px;font-weight:700;line-height:1.25;}
    .price{margin-top:6px;font-size:24px;font-weight:800;color:var(--red2);}
    .chips{display:flex;flex-wrap:wrap;gap:8px;margin-top:14px;}
    .chip{font-size:12.5px;font-weight:500;color:#cfd2db;background:rgba(255,255,255,.06);border:1px solid var(--border);padding:6px 11px;border-radius:999px;}
    .btn{display:flex;align-items:center;justify-content:center;gap:9px;width:100%;padding:15px 18px;border-radius:14px;font-family:inherit;font-size:15.5px;font-weight:600;text-decoration:none;border:none;cursor:pointer;margin-top:12px;}
    .btn:active{transform:scale(.98);}
    .btn-primary{background:linear-gradient(135deg,var(--red2),var(--red));color:#fff;box-shadow:0 10px 26px rgba(202,47,73,.38);}
    .btn-secondary{background:rgba(255,255,255,.04);color:var(--text);border:1.5px solid rgba(255,255,255,.16);}
    .btn svg{width:18px;height:18px;flex:none;}
    .actions{margin-top:20px;}
    footer{margin-top:18px;text-align:center;color:#5f626d;font-size:12px;}
</style>
</head>
<body>
<div class="card">
    <div class="hero">
        <div class="bg" style="background-image:url('%%IMAGE%%')"></div>
        <img src="%%IMAGE%%" alt="%%TITLE%%" onerror="this.style.display='none'" />
        <div class="brandbar"><img src="%%FAVICON%%" alt="" /><span class="c">Car</span><span class="d">Deals</span></div>
    </div>
    <div class="body">
        <h1>%%TITLE%%</h1>
        %%PRICE_BLOCK%%
        %%CHIPS%%
        <div class="actions">
            <a class="btn btn-primary" href="%%APP_LINK%%">
                <svg viewBox="0 0 24 24" fill="none"><path d="M12 3v12m0 0 4-4m-4 4-4-4M5 21h14" stroke="#fff" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>
                Open in CarDeals app
            </a>
            <a class="btn btn-secondary" href="%%PLAY_URL%%">
                <svg viewBox="0 0 24 24" fill="none"><path d="m4 3 13 9-13 9V3Z" stroke="currentColor" stroke-width="1.8" stroke-linejoin="round"/></svg>
                Get the app on Google Play
            </a>
        </div>
        <footer>Buy &amp; sell cars with confidence</footer>
    </div>
</div>
<script>
    var appLink=%%APP_LINK_JSON%%,store="%%PLAY_URL%%",opened=false;
    function mark(){opened=true;}
    document.addEventListener("visibilitychange",function(){if(document.hidden)mark();});
    window.addEventListener("blur",mark);window.addEventListener("pagehide",mark);
    setTimeout(function(){window.location.href=appLink;},1200);
    setTimeout(function(){if(!opened&&!document.hidden)window.location.href=store;},3200);
</script>
</body>
</html>
""";
    }
}
