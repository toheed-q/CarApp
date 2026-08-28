# CarDeals — Share Landing Page

A single static page that powers **car share links**. One page handles every car via
the `?id=` query parameter — you never create a page per car.

## Files to deploy

| File                  | Purpose                                                    |
|-----------------------|------------------------------------------------------------|
| `index.html`          | Fallback landing page (opened with no `?id=`)             |
| `_redirects`          | **Proxies car links to the API preview** — the key file    |
| `cardeals-logo.png`   | CarDeals mark on the fallback card                        |
| `og-image.png`        | Generic preview / fallback image                          |
| `favicon.png`         | Browser tab + preview brand icon                          |

Deploy the **whole `share-web` folder** by drag-and-drop onto Netlify. This is a
pure static deploy — no functions, no CLI.

## Rich per-car previews (how it works)

Social crawlers (WhatsApp, Facebook, X, Instagram) read the Open Graph tags
from the HTML `<head>` and **do not run JavaScript**, so a static page can only
ever show one fixed preview. The dynamic per-car page is therefore rendered by
the **API** (`GET /share/{id}` — see `Apis/DMF_Services/Controllers/ShareController.cs`),
which fetches the car and returns HTML whose `<head>` already carries **that
car's photo, name and price**.

`_redirects` proxies the share URLs to that endpoint (Netlify status 200 = the
visitor stays on this domain but receives the API's HTML):

    /car/:id  ->  {API}/share/:id
    /?id=:id  ->  {API}/share?id=:id

- New clean links:  `https://<site>/car/37`
- Old links still work:  `https://<site>/?id=37`

The **app does not need rebuilding** — existing `?id=` links get the rich
preview automatically. Deploy order: **API first, then this folder.**

### Testing the preview
1. Open `https://<site>/car/37` in a browser — you should see the car card.
2. Paste the link into WhatsApp / a status, or use the Facebook Sharing
   Debugger (`developers.facebook.com/tools/debug`) and **Scrape Again** to
   refresh a cached preview.

## What it does

When someone opens a shared link like `https://<site>/?id=21`:

1. Reads the car id (`21`) from the URL.
2. Tries to open the app via its deep link: `dmfmotors://car/21`.
3. If the app **is** installed → the app opens on that car.
4. If the app **is not** installed → redirects to the Play Store (`com.dmf.services`).

Manual buttons ("Open in app" / "Get the app") are always shown as a fallback in case
the browser blocks the automatic redirect.

## Deploy (Netlify)

Same as the privacy policy: drag-and-drop this `share-web` folder onto Netlify, or
connect it. Netlify serves `index.html` at the site root, so share links look like:

```
https://<your-site>/?id=21
```

Once it's live, share the site URL and it gets wired into the app as the share base URL.

## Config

Edit the constants at the top of the `<script>` block in `index.html` if these ever change:

| Constant     | Value               | Meaning                                  |
|--------------|---------------------|------------------------------------------|
| `APP_SCHEME` | `dmfmotors`         | App's custom URL scheme (`dmfmotors://`) |
| `PACKAGE`    | `com.dmf.services`  | Play Store package name                  |

> Note: the brand is now **CarDeals**, but `APP_SCHEME` and `PACKAGE` are technical
> identifiers baked into the shipped app — they must **stay** `dmfmotors` / `com.dmf.services`.
> Changing them would break every existing share link and installed app.

## Not done yet (app side)

For the link to actually open the app, the app must register the `dmfmotors://` scheme
(Android intent-filter) and handle `dmfmotors://car/{id}` → open that car. That is a
separate app-side change, done when the share feature is implemented.
