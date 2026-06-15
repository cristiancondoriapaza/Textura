# Clasificador de Texturas Satelitales (C# Windows Forms)

Este proyecto es una aplicación de escritorio desarrollada en C# utilizando Windows Forms. Simula los principios básicos del procesamiento de imágenes satelitales, permitiendo al usuario identificar y aislar superficies específicas (como césped, asfalto o tejados) basándose tanto en su **color promedio** como en su **textura** (rugosidad calculada mediante la desviación estándar).

## Características Principales

* **Carga de Imágenes:** Permite importar imágenes desde el equipo local.
* **Muestreo por Clic:** El usuario puede seleccionar una región de 20x20 píxeles simplemente haciendo clic en la imagen.
* **Extracción de Características:** Calcula automáticamente el promedio RGB (Tono) y la desviación estándar (Textura/Rugosidad) de la muestra seleccionada.
* **Almacenamiento Persistente:** Guarda los perfiles de los materiales analizados en una base de datos local SQL Server para su uso posterior.
* **Filtro de Segmentación (Enmascaramiento):** Recorre toda la imagen y aísla las áreas que coinciden con el perfil seleccionado, ocultando el resto de la imagen bajo un fondo negro.

## Requisitos Previos

Para ejecutar y compilar este proyecto, necesitas tener instalado:

* **Visual Studio** (Versión 2019 o superior recomendada) con la carga de trabajo de ".NET desktop development".
* **SQL Server** (LocalDB, Express o superior).
* **.NET Framework** (Compatible con la versión configurada en el proyecto).

## Configuración de la Base de Datos

Antes de ejecutar la aplicación, debes crear la base de datos local que almacenará los perfiles de textura.

1. Abre SQL Server Management Studio (SSMS) o la terminal de tu servidor local.
2. Crea una base de datos llamada `imagenes`.
3. Ejecuta el siguiente script SQL para crear la tabla necesaria:

```sql
CREATE DATABASE imagenes;
GO

USE imagenes;
GO

CREATE TABLE textura (
    id INT IDENTITY(1,1) PRIMARY KEY,
    r INT NOT NULL,
    g INT NOT NULL,
    b INT NOT NULL,
    cr INT NOT NULL,
    cg INT NOT NULL,
    cb INT NOT NULL,
    descripcion VARCHAR(100) NOT NULL
);
