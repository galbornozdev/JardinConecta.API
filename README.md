# 🌱 JardinConecta

**JardinConecta** es una plataforma de comunicación diseñada para **jardines de infantes y familias**, que organiza los comunicados y conversaciones entre docentes y padres en un entorno seguro y estructurado.

La aplicación busca reemplazar el uso desordenado de grupos de mensajería (como WhatsApp) por un sistema donde la información relevante **no se pierde y queda documentada**.

---

# ✨ Problema que resuelve

Muchos jardines utilizan grupos de mensajería para comunicarse con las familias. Esto genera varios problemas:

* mensajes importantes que se pierden entre conversaciones
* dificultad para saber **quién leyó un comunicado**
* falta de historial organizado
* mezcla de información institucional con mensajes personales
* poca trazabilidad de la comunicación

**JardinConecta** propone una solución simple y enfocada.

---

# 🎯 Objetivos del proyecto

* centralizar la comunicación entre jardín y familias
* permitir enviar **comunicados formales**
* registrar **confirmación de lectura**
* mantener un **historial organizado por sala**
* facilitar conversaciones directas entre docentes y familias

---

# 👥 Roles

La plataforma contempla distintos tipos de usuarios:

### Educador

Puede:

* crear comunicados
* editar comunicados
* ver quién leyó cada comunicado
* iniciar conversaciones con familias

### Familia

Puede:

* recibir comunicados
* confirmar lectura
* ver historial de comunicados
* participar en conversaciones con el jardín

---

# 📢 Comunicados

Los comunicados permiten enviar información importante a todos los miembros de una sala.

Cada comunicado incluye:

* título
* contenido
* fecha de publicación
* sala destinataria
* estado de lectura por usuario

El sistema registra:

* quién **vio**
* quién **no vio**
* cuándo fue leído

Esto permite al jardín asegurar que la información fue recibida.

---

# 💬 Conversaciones

Además de los comunicados, la plataforma incluye un módulo de **conversaciones** para comunicación directa.

Características:

* conversaciones entre educador y familia
* historial persistente
* organización por sala
* separación entre mensajes institucionales y conversaciones privadas

---

# 🏗 Arquitectura

El proyecto está diseñado siguiendo principios de **Clean Architecture**.

Capas principales:

```
Domain
Application
Infrastructure
API
```

Principios aplicados:

* separación de responsabilidades
* bajo acoplamiento
* alto nivel de testabilidad
* independencia del framework

---

# 🧰 Stack tecnológico

Backend:

* .NET
* ASP.NET Core Web API
* Entity Framework Core
* SQL Server

Frontend (planned):

* Flutter

Infraestructura (planned):

* Docker
* Cloud deployment
* Push notifications

---

# 🔑 Funcionalidades principales (MVP)

* autenticación de usuarios
* gestión de salas
* creación de comunicados
* edición de comunicados
* registro de lectura de comunicados
* conversaciones entre usuarios

---

# 🚀 Roadmap

Posibles mejoras futuras:

* notificaciones push
* adjuntos en comunicados
* generación de comunicados asistida por IA
* resumen automático de conversaciones
* reportes de comunicación por sala

---

# 🧪 Estado del proyecto

Actualmente en desarrollo como **side project** enfocado en explorar:

* arquitectura backend
* diseño de producto
* comunicación estructurada en entornos educativos

---

# 📌 Motivación

Este proyecto surge como una forma de explorar cómo diseñar software que resuelva problemas reales de comunicación en instituciones educativas, manteniendo un enfoque en **simplicidad, claridad y trazabilidad**.

---

# 📜 Licencia

MIT

---

Si querés, en otro mensaje puedo ayudarte a crear también:

* 🔥 **un README más “viral de GitHub”** (mucho más atractivo visualmente)
* 🧠 **la arquitectura completa del proyecto**
* 📦 **estructura de carpetas recomendada para .NET**
* 🚀 **features que harían que el repo destaque para recruiters**.
