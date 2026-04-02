/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        ink: '#15243d',
        sky: '#eff6ff',
        brand: '#0f766e',
        accent: '#f59e0b',
        card: '#ffffff'
      },
      boxShadow: {
        soft: '0 20px 60px rgba(15, 23, 42, 0.12)'
      },
      backgroundImage: {
        'hero-grid':
          'radial-gradient(circle at top, rgba(20, 184, 166, 0.16), transparent 35%), linear-gradient(135deg, rgba(14, 116, 144, 0.08), rgba(245, 158, 11, 0.08))'
      }
    }
  },
  plugins: [require('@tailwindcss/forms')]
}
