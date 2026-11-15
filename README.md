<!-- local-development:start -->
## Desarrollo local

Consulta [RUNNING.md](RUNNING.md) para ubicar cada componente, abrirlo en su IDE, compilarlo y conocer las comprobaciones realizadas y los requisitos pendientes.
<!-- local-development:end -->

Proyecto MiAgendaUTN
Mi Agenda UTN es una aplicación de gestión de tareas diseñada para estudiantes, enfocada en la organización y priorización de compromisos académicos. Desarrollada con .NET MAUI, la aplicación implementa el patrón de diseño MVVM para garantizar una arquitectura limpia, mantenible y escalable.

Características Principales

CRUD Completo de Tareas: Permite Crear, Leer, Actualizar y Eliminar tareas académicas.

Persistencia Local (JSON): Todos los datos de las tareas se guardan en el dispositivo en formato JSON, asegurando que la información se mantenga entre sesiones.

Arquitectura MVVM: Separación estricta de la interfaz de usuario (View), la lógica de presentación (ViewModel) y los datos (Model).

Sincronización en Tiempo Real: Utiliza ObservableCollection y un ViewModel Singleton para garantizar que los cambios se reflejen instantáneamente en todas las vistas (Dashboard y Lista de Tareas).

Navegación Fluida: Implementación de AppShell para una navegación basada en pestañas y registro de rutas dinámicas.

Modo Oscuro/Claro: Módulo de ajustes para alternar el tema visual de la aplicación.

Interacciones Intuitivas: Uso de gestos deslizables (Swipe Gestures) para finalizar o eliminar tareas rápidamente desde la lista.

Tecnologías Utilizadas

Framework: .NET MAUI (Multi-platform App UI)

Lenguaje de Programación: C#

Patrón de Arquitectura: MVVM (Model-View-ViewModel)

Persistencia de Datos: Serialización/Deserialización JSON local.

🛠️ Estructura del Proyecto

El proyecto está organizado en las siguientes carpetas principales, siguiendo el patrón MVVM:

MiAgendaUTN/
├── Models/              # Define las estructuras de datos (TaskModel, SettingsModel).
├── ViewModels/          # Contiene la lógica de negocio y presentación (TaskViewModel, SettingsViewModel).
├── Views/               # Archivos XAML de la interfaz de usuario (TasksPage, TaskFormPage, SettingsPage).
└── Services/            # Capa de servicios para operaciones externas (TaskDataService para JSON).


Configuración y Ejecución

Requisitos Previos

Instalar Visual Studio 2022 (versión 17.3 o superior).

Asegurarse de tener la carga de trabajo ".NET Multi-platform App UI development" (.NET MAUI) instalada.

Tener instalado el SDK de .NET más reciente (actualmente .NET 8).

Pasos para Ejecutar

Clonar el repositorio:

git clone [URL_DEL_REPOSITORIO]
cd MiAgendaUTN


Abrir la solución (MiAgendaUTN.sln) en Visual Studio.

Seleccionar la plataforma de destino (Android, Windows, iOS, Mac Catalyst).

Ejecutar el proyecto (F5 o el botón de Run en Visual Studio).

Flujo de Navegación

El sistema utiliza AppShell y presenta 4 secciones principales accesibles desde el TabBar:

Home (Dashboard): Muestra un resumen de tareas, KPIs (pendientes, completadas, para hoy) y listas filtradas.

Tasks (Lista de Tareas): Vista principal con la colección completa de tareas y gestos de interacción.

New Task (Nueva Tarea): Formulario para la creación de una nueva tarea.

Settings (Ajustes): Módulo para controlar el tema visual (Modo Oscuro/Claro).

© 2024 Mi Agenda UTN. Desarrollado con .NET MAUI.
