# NightRaven Web

React + Vite frontend for NightRaven player and admin surfaces.

## Commands

```bash
npm run dev
npm run build
npm run lint
npm run generate:api
```

The dev server runs on `http://localhost:5173` and proxies `/api`, `/metrics`,
and `/openapi` to the ASP.NET Core backend on `http://localhost:5265`.

## Structure

```text
src/
  api/        HTTP helpers and typed endpoint wrappers
  app/        root providers and route composition
  features/   player, admin, and home feature screens
  shared/     reusable layout and display components
  styles/     global tokens and app shell styles
```

The current UI is intentionally a scaffold. It keeps player and admin routes
separate, leaves backend auth enforcement to the server, and gives future API
work stable places to land.

## Font credits

The player portal uses `UOFont.ttf` from Jackkv/UOFont as a local asset:
https://github.com/Jackkv/UOFont

UOFont is a fan project and is not affiliated with Electronic Arts or Broadsword.
The themed cursor PNGs also come from Jackkv/UOFont's demo page assets.
