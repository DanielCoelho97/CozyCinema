/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        cinema: {
          // Fundo principal — vinho escuro
          background: '#1E0F14',
          // Superfícies elevadas (cards) — cinza estofado, levemente amadeirado
          surface: '#2B2226',
          surfaceElevated: '#352A2F',
          // Destaque primário — bordô/burgundy
          primary: '#7A1F2B',
          primaryHover: '#921F2E',
          // Acento — amarelo pipoca / âmbar
          accent: '#E8A83C',
          accentHover: '#F2B958',
          // Texto — bege macio
          text: '#F1E4D8',
          textMuted: '#C9B8AC',
          // Estados
          success: '#5C8A5A',
          danger: '#C1443C',
        },
      },
      boxShadow: {
        cozy: '0 8px 24px -6px rgba(0, 0, 0, 0.45)',
        cozyLg: '0 16px 40px -8px rgba(0, 0, 0, 0.55)',
        glowAccent: '0 0 32px 4px rgba(232, 168, 60, 0.35)',
      },
      fontFamily: {
        heading: ['Poppins', 'Nunito', 'sans-serif'],
        body: ['Inter', 'sans-serif'],
      },
    },
  },
  plugins: [],
}
