# Persistencia con SQLite

## Ubicacion
- Cadena de conexion: ConnectionStrings:AulaPedidos (por defecto Data Source=App_Data/aulapedidos.db).
- Ruta relativa al directorio de trabajo; con dotnet run o Visual Studio es src/AulaPedidos.Api/App_Data/.
- La carpeta se crea automaticamente y esta excluida de Git (App_Data/, *.db).
- Otra ruta (contenedor, servidor): variable de entorno ConnectionStrings__AulaPedidos.

## Migraciones al arrancar
- En Development se aplican automaticamente.
- En otros entornos: Database:MigrateOnStartup=true o dotnet ef database update.

## Comandos (requiere: dotnet tool install --global dotnet-ef)
- Crear: dotnet ef migrations add NOMBRE --project src/AulaPedidos.Infrastructure --startup-project src/AulaPedidos.Api --output-dir Persistence/Migrations
- Aplicar: dotnet ef database update --project src/AulaPedidos.Infrastructure --startup-project src/AulaPedidos.Api
- Revertir: dotnet ef database update MIGRACION_ANTERIOR --project src/AulaPedidos.Infrastructure --startup-project src/AulaPedidos.Api
- Eliminar la ultima (sin aplicar): dotnet ef migrations remove --project src/AulaPedidos.Infrastructure --startup-project src/AulaPedidos.Api

## Limitaciones de SQLite
- decimal se guarda como TEXT: valido para guardar y comparar por igualdad; ordenar o agregar en SQL puede dar resultados incorrectos.
- AlterColumn y DropColumn reconstruyen la tabla; revisar migraciones futuras.
