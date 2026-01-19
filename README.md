# ⭐ StarGaze Weather Conditions

> A beautiful, intelligent weather application for stargazers and astrophotographers

[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Blazor WebAssembly](https://img.shields.io/badge/Blazor-WebAssembly-512BD4?logo=blazor)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Build Status](https://github.com/yourusername/StargazeWeatherConditions/workflows/Deploy%20to%20GitHub%20Pages/badge.svg)](https://github.com/yourusername/StargazeWeatherConditions/actions)

**StarGaze Weather Conditions** helps amateur astronomers, astrophotographers, and casual stargazers determine optimal conditions for nighttime sky observation. Get detailed weather forecasts focused on what matters most: cloud cover, moon phase, visibility, humidity, and astronomical twilight times—all with intelligent recommendations for your observation sessions.

🌐 **[Live Demo](https://yourusername.github.io/StargazeWeatherConditions/)** (Update with your actual GitHub Pages URL)

---

## ✨ Features

### 🌤️ **Comprehensive Weather Forecasts**
- **3-day/3-night hourly forecasts** specifically for stargazing conditions
- Real-time weather data from [WeatherAPI.com](https://www.weatherapi.com)
- Cloud cover, humidity, visibility, temperature, wind speed
- Precipitation probability and weather conditions

### 🌙 **Astronomical Data**
- **Moon phase and illumination** percentage
- Moonrise and moonset times
- **Twilight calculations**: Civil, Nautical, and Astronomical
- Optimal viewing windows between twilights
- Visual timeline showing sun/moon positions

### 💡 **Intelligent Recommendations**
- **Smart scoring system** weighing multiple factors:
  - Cloud Cover (35%) - Most critical
  - Moon Illumination (25%) - Affects dark sky targets
  - Humidity (15%) - Impacts transparency
  - Visibility (15%) - Atmospheric clarity
  - Light Pollution (10%) - Site quality
- **Ratings**: Excellent, Good, Fair, Poor
- **Specialized scores** for Deep Sky Objects vs. Planetary/Lunar observation
- Contextual advice based on conditions

### 🗺️ **Location Services**
- Browser geolocation with permission handling
- Location search with autocomplete
- Support for city names, coordinates, and postal codes
- Session persistence

### 🎨 **Beautiful User Interface**
- **Dark/Light themes** with system preference detection
- Fully responsive design (mobile-first)
- Smooth animations and transitions
- WCAG 2.1 AA accessibility compliant

### ⚙️ **User Preferences**
- Temperature units (°F/°C)
- Distance units (miles/km)
- Wind speed units (mph/kph)
- Time format (12/24 hour)
- Custom API key support

### ⚡ **Performance**
- Client-side caching (2-hour TTL)
- Graceful offline support
- Ahead-of-Time (AOT) compilation
- Optimized for fast loading

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- A modern web browser (Chrome, Firefox, Safari, Edge)
- [WeatherAPI.com](https://www.weatherapi.com) API key (free tier available)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/StargazeWeatherConditions.git
   cd StargazeWeatherConditions
   ```

2. **Configure API Key**
   
   Edit `src/StargazeWeatherConditions/wwwroot/appsettings.json`:
   ```json
   {
     "WeatherApi": {
       "ApiKey": "YOUR_API_KEY_HERE",
       "BaseUrl": "https://api.weatherapi.com/v1/"
     }
   }
   ```
   
   Get your free API key from [WeatherAPI.com](https://www.weatherapi.com/signup.aspx) (1M calls/month free tier).

3. **Restore dependencies**
   ```bash
   dotnet restore
   ```

4. **Run the application**
   ```bash
   dotnet run --project src/StargazeWeatherConditions
   ```

5. **Open in browser**
   
   Navigate to `https://localhost:5001` (or the port shown in terminal)

---

## 🧪 Testing

The project includes comprehensive unit tests using xUnit, Moq, and bUnit.

### Run all tests
```bash
dotnet test
```

### Run tests with coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Test coverage
- **Target**: 80%+ overall coverage
- **Services**: 90%+ coverage
- **Utilities**: 95%+ coverage
- **Components**: 70%+ coverage

---

## 📦 Deployment

The application is configured for deployment to **GitHub Pages** using GitHub Actions.

### Automatic Deployment

1. **Set up GitHub repository secrets**:
   - Go to Settings → Secrets → Actions
   - Add `WEATHERAPI_KEY` with your API key

2. **Enable GitHub Pages**:
   - Go to Settings → Pages
   - Set source to "Deploy from a branch"
   - Select `gh-pages` branch, `/ (root)` folder

3. **Push to main branch**:
   ```bash
   git push origin main
   ```
   
   The GitHub Actions workflow will automatically:
   - Build the project
   - Run all tests
   - Inject the API key
   - Publish with AOT compilation
   - Deploy to GitHub Pages

4. **Access your deployed app**:
   `https://yourusername.github.io/StargazeWeatherConditions/`

### Manual Build

```bash
# Publish with AOT compilation
dotnet publish src/StargazeWeatherConditions/StargazeWeatherConditions.csproj \
  -c Release \
  -o release \
  -p:RunAOTCompilation=true

# Output will be in release/wwwroot/
```

---

## 🏗️ Project Structure

```
StargazeWeatherConditions/
├── .github/
│   ├── copilot-instructions.md    # Development guidelines
│   └── workflows/
│       └── deploy.yml              # CI/CD pipeline
├── src/
│   └── StargazeWeatherConditions/
│       ├── Components/             # Reusable Blazor components
│       │   ├── Layout/            # MainLayout, NavBar, Footer
│       │   ├── Weather/           # Weather display components
│       │   ├── Astronomy/         # Moon, twilight, sun components
│       │   ├── Recommendations/   # Scoring and recommendation UI
│       │   ├── Location/          # Search and geolocation
│       │   └── Settings/          # User preferences
│       ├── Pages/                  # Routable pages
│       ├── Services/               # Business logic and API services
│       ├── Models/                 # Data models and DTOs
│       ├── Utilities/              # Helper classes and calculations
│       └── wwwroot/                # Static assets
├── tests/
│   └── StargazeWeatherConditions.Tests/
│       ├── Services/               # Service unit tests
│       ├── Utilities/              # Utility unit tests
│       └── Components/             # bUnit component tests
├── docs/
│   └── PRD.md                      # Product Requirements Document
├── LICENSE
└── README.md
```

---

## 🛠️ Technology Stack

| Category | Technology |
|----------|------------|
| **Framework** | .NET 10, Blazor WebAssembly |
| **Language** | C# 13 |
| **UI** | Razor components, CSS custom properties |
| **APIs** | WeatherAPI.com, Light Pollution Map API |
| **Testing** | xUnit, Moq, bUnit |
| **Storage** | Browser localStorage/sessionStorage |
| **Hosting** | GitHub Pages |
| **CI/CD** | GitHub Actions |
| **HTTP** | HttpClient with Polly retry policies |

---

## 📖 Usage Guide

### Finding Your Location

1. **Use Geolocation**: Click "Use My Location" to automatically detect your coordinates
2. **Search**: Type a city name, coordinates, or postal code in the search box
3. **Select**: Choose from the autocomplete suggestions

### Reading the Forecast

- **Overall Rating**: Large badge showing Excellent/Good/Fair/Poor conditions
- **Hourly Cards**: Scroll horizontally to see each hour's conditions
- **Timeline**: Visual representation of twilight phases and moon position
- **Factor Breakdown**: Individual scores for cloud cover, moon, humidity, etc.

### Understanding Recommendations

- **DSO Score**: Optimized for Deep Sky Objects (galaxies, nebulae, star clusters)
  - Best with new moon and dark skies
- **Planetary Score**: Optimized for planets and lunar observation
  - Tolerates bright moon, needs clear skies

### Customizing Settings

1. Click the **Settings** icon in the header
2. Toggle **Dark/Light theme**
3. Set **unit preferences** (temperature, distance, wind, time)
4. (Optional) Add **custom API key** for personal rate limits

---

## 🌟 Screenshots

### Desktop - Dark Theme
![Desktop Dark](docs/screenshots/desktop-dark.png) *(Add screenshot)*

### Mobile - Light Theme
![Mobile Light](docs/screenshots/mobile-light.png) *(Add screenshot)*

### Twilight Timeline
![Timeline](docs/screenshots/timeline.png) *(Add screenshot)*

---

## 🤝 Contributing

Contributions are welcome! Please follow these guidelines:

1. **Fork the repository**
2. **Create a feature branch**: `git checkout -b feature/amazing-feature`
3. **Follow coding standards**: See [copilot-instructions.md](.github/copilot-instructions.md)
4. **Write tests** for new functionality
5. **Ensure all tests pass**: `dotnet test`
6. **Commit changes**: `git commit -m 'Add amazing feature'`
7. **Push to branch**: `git push origin feature/amazing-feature`
8. **Open a Pull Request**

### Coding Standards

- Use C# 13 features (file-scoped namespaces, primary constructors)
- Follow nullable reference type conventions
- Maintain 80%+ test coverage
- Use async/await for I/O operations
- Include XML documentation for public APIs

See [.github/copilot-instructions.md](.github/copilot-instructions.md) for detailed development guidelines.

---

## 📋 Roadmap

### Version 1.0 (Current)
- ✅ 3-day hourly forecasts
- ✅ Astronomical data (moon, twilight)
- ✅ Intelligent recommendation engine
- ✅ Location search and geolocation
- ✅ Dark/Light themes
- ✅ Unit preferences
- ✅ Client-side caching

### Future Considerations
- 📍 Saved favorite locations
- 🔔 Weather alerts for excellent conditions
- 📊 Historical conditions data
- 🔭 Equipment-specific recommendations
- 📅 Celestial events calendar integration
- 🌐 Progressive Web App (PWA) support
- 🌍 Multi-language support

---

## ❓ FAQ

### **Q: Is this free to use?**
**A:** Yes! The application is open-source (MIT license) and uses free-tier APIs. You can deploy your own instance at no cost using GitHub Pages.

### **Q: Why can I see the API key in the browser?**
**A:** Blazor WebAssembly runs entirely in the browser, so all configuration is visible. We use the free tier of WeatherAPI.com which allows this. For production with paid tiers, consider a backend proxy.

### **Q: How accurate are the twilight calculations?**
**A:** Twilight times are calculated using standard solar position algorithms with <1 minute accuracy for most locations.

### **Q: What's the light pollution data source?**
**A:** Currently using Light Pollution Map API. If unavailable, the app estimates based on typical suburban conditions (Bortle 5).

### **Q: Does this work offline?**
**A:** Partially. Cached weather data is available for 2 hours offline. Future versions may include full PWA support.

### **Q: Can I use my own API key?**
**A:** Yes! Go to Settings and enter your custom WeatherAPI.com key. It will be stored in browser localStorage and take precedence over the default key.

### **Q: Which browsers are supported?**
**A:** All modern browsers: Chrome, Firefox, Safari, Edge (latest 2 versions). Requires JavaScript enabled.

---

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

```
MIT License

Copyright (c) 2026 Jeremy Bray

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND...
```

---

## 🙏 Acknowledgments

- **[WeatherAPI.com](https://www.weatherapi.com)** - Weather data provider
- **[Light Pollution Map](https://www.lightpollutionmap.info/)** - Light pollution data
- **[.NET Team](https://github.com/dotnet)** - Blazor WebAssembly framework
- **Astronomy Community** - Inspiration and domain expertise

---

## 📞 Contact

**Jeremy Bray** - Project Maintainer

- GitHub: [@yourusername](https://github.com/yourusername) *(Update with actual username)*
- Issues: [GitHub Issues](https://github.com/yourusername/StargazeWeatherConditions/issues)

---

## ⭐ Star This Repository

If you find this project useful, please consider giving it a star! It helps others discover the project and motivates continued development.

---

**Built with ❤️ for the stargazing community**

*Clear skies and happy observing!* 🔭✨
