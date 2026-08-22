# CarDeals — Share Landing Page

A single static page that powers **car share links**. One page handles every car via
the `?id=` query parameter — you never create a page per car.

## Files to deploy

| File                 | Purpose                                            |
|----------------------|----------------------------------------------------|
| `index.html`         | The landing / redirect page                        |
| `cardeals-logo.png`  | CarDeals mark shown on the card                    |
| `og-image.png`       | Social share preview (WhatsApp / Facebook / X)     |
| `favicon.png`        | Browser tab icon                                   |

Deploy the **whole `share-web` folder** so these assets resolve.

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
