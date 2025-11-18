# ETL-ELECT-1
Electiva 1 ARQUITECTURA
# Proceso ETL - Worker Service

## Descripción

Worker Service en .NET 8 que implementa un proceso ETL (Extract, Transform, Load) escalable y robusto. El proyecto extrae datos de múltiples fuentes (CSV, Base de Datos, API REST), los transforma y los carga en una base de datos analítica.

## Requisitos del Sistema

### Opcionales (para funcionamiento completo):
- **SQL Server**: Para bases de datos fuente y analítica
- **Archivos CSV**: En la carpeta `data\csv\`
- **API REST**: Configurada en `appsettings.json`

### Mínimos (para demostración):
- **.NET 8 SDK**: Para compilar y ejecutar
- **Archivos CSV**: Al menos un archivo CSV en `data\csv\` para demostrar la extracción

## Configuración

### 1. Base de Datos (Opcional)

Si tienes SQL Server disponible, configura las conexiones en `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "AnalyticsDb": "Server=TU_SERVIDOR;Database=AnalyticsDB;Trusted_Connection=true;TrustServerCertificate=true;",
    "SourceDb": "Server=TU_SERVIDOR;Database=CustomerOrdersDB;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

**Nota**: Si no tienes SQL Server disponible, el proyecto funcionará igualmente:
- La extracción CSV funcionará normalmente
- Los datos se guardarán en el área de staging (archivos JSON)
- La fase de Load mostrará un warning pero no detendrá la aplicación

### 2. Archivos CSV

Coloca tus archivos CSV en la carpeta `data\csv\` con los siguientes nombres:
- `customers.csv`
- `products.csv`
- `orders.csv`
- `order_details.csv`

**Estructura de ejemplo para customers.csv:**
```csv
CustomerID,FirstName,LastName,Email,Phone,City,Country
1,Juan,Pérez,juan@email.com,1234567890,México,México
```

### 3. API REST (Opcional)

Si tienes una API REST disponible, configura en `appsettings.json`:

```json
{
  "DataSources": {
    "ApiBaseUrl": "https://tu-api.com",
    "ApiEndpoints": {
      "Comments": "/api/comments",
      "Reviews": "/api/reviews"
    },
    "ApiKey": "TU_API_KEY"
  }
}
```

## Ejecución

### Compilar
```bash
dotnet build
```

### Ejecutar
```bash
dotnet run --project ProcesoETL/ProcesoETL.csproj
```

El servicio se ejecutará como un Worker Service y ejecutará el proceso ETL según el intervalo configurado (por defecto cada 60 minutos).

## Funcionamiento Sin Base de Datos

El proyecto está diseñado para funcionar **sin base de datos conectada**:

1. **Extracción CSV**: ✅ Funciona sin base de datos
   - Lee archivos CSV desde `data\csv\`
   - Guarda datos en staging (archivos JSON en carpeta `staging`)

2. **Extracción Base de Datos**: ⚠️ Requiere base de datos
   - Si no hay base de datos, se registra un error y continúa
   - No detiene el proceso ETL

3. **Extracción API**: ⚠️ Requiere API disponible
   - Si la API no está disponible, se registra un error y continúa
   - No detiene el proceso ETL

4. **Transformación**: ✅ Funciona sin base de datos
   - Carga datos del staging
   - Aplica transformaciones
   - Guarda datos transformados de vuelta al staging

5. **Carga a Base de Datos**: ⚠️ Requiere base de datos
   - Si no hay base de datos, muestra warning pero continúa
   - Los datos quedan guardados en staging y se pueden cargar después

## Área de Staging

Los datos extraídos y transformados se guardan en la carpeta `staging` como archivos JSON:
- `Customers_YYYYMMDD_HHMMSS.json`
- `Products_YYYYMMDD_HHMMSS.json`
- `Orders_YYYYMMDD_HHMMSS.json`
- `OrderDetails_YYYYMMDD_HHMMSS.json`
- `Reviews_YYYYMMDD_HHMMSS.json`
- `Comments_YYYYMMDD_HHMMSS.json`
- Y versiones transformadas: `Customers_Transformed_YYYYMMDD_HHMMSS.json`, etc.

## Logs

Los logs se guardan en:
- **Consola**: Muestra eventos en tiempo real
- **Archivo**: `logs/etl-YYYYMMDD.log`

## Arquitectura

- **Clean Architecture**: Separación en capas (Core, Application, Infrastructure)
- **SOLID Principles**: Interfaces y abstracciones
- **Dependency Injection**: Servicios registrados en Program.cs
- **Async/Await**: Operaciones asíncronas para mejor rendimiento
- **Procesamiento Paralelo**: Configurable en `appsettings.json`

## Componentes Principales

- **CsvExtractor**: Extrae datos de archivos CSV
- **DatabaseExtractor**: Extrae datos de base de datos relacional
- **ApiExtractor**: Extrae datos de API REST
- **StagingService**: Gestiona el almacenamiento temporal
- **DataLoader**: Carga datos a la base de datos analítica
- **ETLPipeline**: Orquesta el proceso ETL completo

## Entrega del Proyecto

El proyecto puede entregarse **sin base de datos conectada** y funcionará correctamente:

✅ **Funcionará al 100%**:
- Extracción de CSV
- Transformación de datos
- Guardado en staging
- Logging completo
- Arquitectura demostrada

⚠️ **Mostrará warnings pero continuará**:
- Extracción de base de datos (si no está disponible)
- Extracción de API (si no está disponible)
- Carga a base de datos (si no está disponible)

Los datos estarán disponibles en la carpeta `staging` en formato JSON, listos para ser cargados cuando la base de datos esté disponible.

