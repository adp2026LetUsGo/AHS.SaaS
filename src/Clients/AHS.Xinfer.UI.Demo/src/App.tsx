import PredictionPage from './pages/PredictionPage';
// AHS.Web.Common — Sovereign Elite Design System (single source of truth per ADR-008)
import '@ahs-web-common/css/sovereign-elite.css';

function App() {
  return (
    // .ahs-app applies the sovereign baseline: dark bg, gradient, font
    <div className="ahs-app">
      <PredictionPage />
    </div>
  )
}

export default App
