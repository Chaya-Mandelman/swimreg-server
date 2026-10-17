# בנייה מתיקיית השורש של ה-repo:  docker build -f src/SwimReg.API/Dockerfile -t swimreg-api .

# ---------- שלב 1: build עם ה-SDK ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# קודם רק קבצי ה-csproj, כדי ש-restore יישמר ב-cache כל עוד התלויות לא השתנו
COPY src/SwimReg.Domain/SwimReg.Domain.csproj src/SwimReg.Domain/
COPY src/SwimReg.Application/SwimReg.Application.csproj src/SwimReg.Application/
COPY src/SwimReg.Infrastructure/SwimReg.Infrastructure.csproj src/SwimReg.Infrastructure/
COPY src/SwimReg.API/SwimReg.API.csproj src/SwimReg.API/
RUN dotnet restore src/SwimReg.API/SwimReg.API.csproj

COPY src/ src/
RUN dotnet publish src/SwimReg.API/SwimReg.API.csproj -c Release -o /app/publish --no-restore

# ---------- שלב 2: runtime קטן בלי SDK ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# תיקיית הלוגים של NLog צריכה הרשאת כתיבה למשתמש app (הקונטיינר לא רץ כ-root)
RUN mkdir -p /app/logs && chown app /app/logs
USER app

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# Render מעביר את הפורט במשתנה PORT. בלעדיו: 8080
ENTRYPOINT ["sh", "-c", "ASPNETCORE_HTTP_PORTS=${PORT:-8080} exec dotnet SwimReg.API.dll"]
