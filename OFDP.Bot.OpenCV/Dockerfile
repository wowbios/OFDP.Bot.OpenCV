FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["OFDP.Bot.OpenCV/OFDP.Bot.OpenCV.csproj", "OFDP.Bot.OpenCV/"]
RUN dotnet restore "OFDP.Bot.OpenCV/OFDP.Bot.OpenCV.csproj"
COPY . .
WORKDIR "/src/OFDP.Bot.OpenCV"
RUN dotnet build "OFDP.Bot.OpenCV.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OFDP.Bot.OpenCV.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OFDP.Bot.OpenCV.dll"]
