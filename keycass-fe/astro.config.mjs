// @ts-check
import { defineConfig } from 'astro/config';

import tailwindcss from '@tailwindcss/vite';

import alpinejs from '@astrojs/alpinejs';

// https://astro.build/config
export default defineConfig({
  site: 'https://renatocassino.github.io',
  base: 'keycass-trompete',

  vite: {
    plugins: [tailwindcss()]
  },

  integrations: [alpinejs()]
});