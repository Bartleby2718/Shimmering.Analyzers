import {themes as prismThemes} from 'prism-react-renderer';
import type {Config} from '@docusaurus/types';
import type * as Preset from '@docusaurus/preset-classic';

// This runs in Node.js - Don't use client-side code here (browser APIs, JSX...)

const config: Config = {
  title: 'Shimmering.Analyzers',
  tagline: 'An opinionated set of Roslyn analyzers promoting best practices in .NET',
  favicon: 'img/favicon.ico',

  // Set the production url of your site here
  url: 'https://bartleby2718.github.io',
  // Set the /<baseUrl>/ pathname under which your site is served
  // For GitHub pages deployment, it is often '/<projectName>/'
  baseUrl: '/Shimmering.Analyzers/',

  // GitHub pages deployment config.
  // If you aren't using GitHub pages, you don't need these.
  organizationName: 'Bartleby2718', // Usually your GitHub org/user name.
  projectName: 'Shimmering.Analyzers', // Usually your repo name.

  onBrokenLinks: 'throw',
  onBrokenMarkdownLinks: 'warn',

  // Even if you don't use internationalization, you can use this field to set
  // useful metadata like html lang. For example, if your site is Chinese, you
  // may want to replace "en" with "zh-Hans".
  i18n: {
    defaultLocale: 'en',
    locales: ['en'],
  },

  presets: [
    [
      'classic',
      {
        docs: {
          sidebarPath: './sidebars.ts',
          exclude: ['AnalyzerDocumentationTemplate.md'],
          // Please change this to your repo.
          // Remove this to remove the "edit this page" links.
          editUrl:
            'https://github.com/Bartleby2718/Shimmering.Analyzers/blob/main/site/',
        },
        blog: false, // Disable blog
        theme: {
          customCss: './src/css/custom.css',
        },
      } satisfies Preset.Options,
    ],
  ],

  plugins: [require.resolve('docusaurus-lunr-search')],
  
  themeConfig: {
    navbar: {
      title: 'Shimmering.Analyzers',
      logo: {
        alt: 'Shimmering.Analyzers Logo',
        src: 'img/icon.png',
      },
      items: [
        {
          type: 'docSidebar',
          sidebarId: 'tutorialSidebar',
          position: 'left',
          label: 'Reference',
        },
      ],
    },
    footer: {
      style: 'dark',
      links: [
        {
          title: 'Docs',
          items: [
            {
              label: 'For users',
              to: '/docs/AllRules',
            },
            {
              label: 'For contributors',
              to: '/docs/CONTRIBUTING',
            },
          ],
        },
        {
          title: 'More',
          items: [
            {
              label: 'GitHub',
              href: 'https://github.com/Bartleby2718/Shimmering.Analyzers',
            },
          ],
        },
      ],
      copyright: `Copyright © ${new Date().getFullYear()} Jihoon Park. Built with Docusaurus.`,
    },
    prism: {
      additionalLanguages: ['csharp'],
      theme: prismThemes.github,
      darkTheme: prismThemes.dracula,
    },
  } satisfies Preset.ThemeConfig,
};

export default config;
