# JonsBlogBE

## Tailwind CSS

Tailwind source is `JonsblogBE/WebPages/style.css` and generated output is `JonsblogBE/WebPages/style.generated.css`.

Build once:

```bash
cd JonsblogBE
npm install
npm run build:css
```

Watch manually:

```bash
cd JonsblogBE
npm run watch:css
```

When running the API in `Development`, the app now attempts to start `npm run watch:css` automatically on startup and stops it on shutdown.
