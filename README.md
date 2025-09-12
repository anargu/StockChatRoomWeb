# StockChatRoomWeb

Una aplicación de chat multi-sala en tiempo real con funcionalidad integrada de consulta de precios de acciones. Los usuarios pueden crear salas de chat, enviar mensajes y consultar precios de acciones usando comandos.

## Features

- [x] Permitir a los usuarios registrados iniciar sesión y conversar con otros
usuarios en una sala de chat.
- [x] Permitir a los usuarios publicar mensajes como comandos en la sala de chat
con el siguiente formato: /stock=<código_de_acción>
- [x] Crear un bot desacoplado que llame a una API utilizando el
código_de_acción como parámetro (ejemplo:
https://stooq.com/q/l/?s=aapl.us&f=sd2t2ohlcv&h&e=csv, donde aapl.us es
el código_de_acción)
- [x] El bot debe analizar el archivo CSV recibido y luego enviar un mensaje de
vuelta a la sala de chat utilizando un gestor de mensajes como RabbitMQ. El
mensaje será una cotización bursátil bajo el siguiente formato: “La
cotización para APPL.US es $93,42 por acción.” El propietario del mensaje
será el bot.
- [x] Mostrar los mensajes ordenados por tiempo y solo los últimos 50.
- [x] Realizar pruebas unitarias de la funcionalidad de preferencia.

## Features/Funcionalidades Opcionales Implementadas

- [x] Generar más de una sala de chat.
- [x] Utilizar .NET Identity para la autenticación de usuarios.
- [x] Manejar mensajes que no se entienden o cualquier excepción que se produzca dentro del bot.
- [ ] Construir un instalador.

## Stack Tecnológico

### Backend (.NET 8)
- **ASP.NET Core Web API** - Servicios API RESTful
- **Entity Framework Core 8.0.11** - ORM con PostgreSQL
- **SignalR** - Comunicación bidireccional en tiempo real
- **ASP.NET Core Identity** - Autenticación y autorización de usuarios
- **JWT Bearer Authentication** - Seguridad basada en tokens
- **RabbitMQ** - Message broker para la funcionalidad del bot de acciones
- **Docker** - Containerización

### Frontend (Vue.js 3)
- **Vue.js 3** con Composition API
- **Pinia** - Gestión de estado
- **Vue Router** - Enrutamiento del lado del cliente
- **Tailwind CSS** - Framework CSS utility-first
- **Axios** - Cliente HTTP
- **Microsoft SignalR Client** - Comunicación en tiempo real

### Servicios Externos
- **Stooq API** - Proveedor de datos de precios de acciones
- **PostgreSQL** - Base de datos principal
- **RabbitMQ** - Sistema de cola de mensajes

## Patrones de Arquitectura

El proyecto sigue los principios de Clean Architecture con clara separación de responsabilidades:

```
StockChatRoomWeb/
├── StockChatRoomWeb.Core/          # Entidades del dominio e interfaces
├── StockChatRoomWeb.Infrastructure/ # Acceso a datos y servicios externos
├── StockChatRoomWeb.Api/           # Controladores Web API y hubs SignalR
├── StockChatRoomWeb.Shared/        # DTOs compartidos y utilidades comunes
├── StockChatRoomWeb.Tests/         # Pruebas unitarias e integración
└── StockChatRoomWeb.Frontend/      # Aplicación frontend Vue.js
```

## Diseño de Base de Datos

### Entidades Principales
- **User** (ASP.NET Identity) - Cuentas de usuario y autenticación
- **ChatRoom** - Salas de chat individuales con relación al creador
- **ChatMessage** - Mensajes con ChatRoomId nullable (NULL = Chat Global)

### Relaciones Clave
- User → ChatRooms (Uno-a-Muchos)
- ChatRoom → ChatMessages (Uno-a-Muchos)
- User → ChatMessages (Uno-a-Muchos)
- El Chat Global usa `ChatRoomId = NULL`

## Iniciar el sistema

### Prerrequisitos
- .NET 8 SDK
- Node.js 18+ y npm
- Docker y Docker Compose
- PostgreSQL (o usar Docker)
- RabbitMQ (o usar Docker)

### Ejecutar con Docker Compose

1. **Clonar el repositorio**
   ```bash
   git clone <repository-url>
   cd StockChatRoomWeb
   ```

2. **Iniciar todos los servicios** (incluye migraciones automáticas)
   ```bash
   docker-compose up -d
   ```

3. **Acceder a la aplicación**
   - Aplicación completa: http://localhost:8080
   - API: http://localhost:8080/api
   - SignalR Hub: http://localhost:8080/chatHub

### Ejecutar para Desarrollo

#### Backend
```bash
cd StockChatRoomWeb.Api
dotnet restore
dotnet run
```

#### Frontend
```bash
cd StockChatRoomWeb.Frontend
npm install
npm run dev
```

#### Configuración de Base de Datos
```bash
cd StockChatRoomWeb.Api
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Endpoints de API

### Autenticación
- `POST /api/auth/login` - Inicio de sesión de usuario
- `POST /api/auth/register` - Registro de usuario

### Salas de Chat
- `GET /api/chatroom` - Obtener todas las salas de chat
- `GET /api/chatroom/{id}` - Obtener sala de chat específica
- `POST /api/chatroom` - Crear nueva sala de chat

### Mensajes
- `GET /api/chat/messages?chatRoomId={id}&count={count}` - Obtener mensajes
- `POST /api/chat/messages` - Enviar mensaje

### SignalR Hub
- **URL del Hub**: `/chatHub`
- **Métodos**: 
  - `JoinRoom(roomId)` - Unirse a sala específica
  - `LeaveRoom(roomId)` - Salir de sala específica
  - `ReceiveMessage` - Escuchar nuevos mensajes

## Comandos del Bot de Acciones

Los usuarios pueden consultar precios de acciones escribiendo comandos en el chat:
- `/stock=AAPL` - Obtener precio de acciones de Apple

El bot procesa estos comandos de forma asíncrona y responde en la misma sala de chat.


## Pruebas

Ejecutar pruebas unitarias:
```bash
cd StockChatRoomWeb.Tests
dotnet test
```

## Detalles de Estructura del Proyecto

### Servicios Backend
- **ChatService** - Funcionalidad principal del chat
- **AuthService** - Autenticación de usuario
- **StockResponseBackgroundService** - Procesa consultas de acciones
- **SignalR ChatHub** - Hub de comunicación en tiempo real
