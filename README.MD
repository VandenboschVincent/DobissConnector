
# Dobiss MQTT Bridge for Home Assistant

A lightweight .NET bridge for connecting a **Dobiss light control system** to an **MQTT broker**, enabling seamless integration with **Home Assistant**. The application also includes a **REST API** to monitor and control lights directly when MQTT is not used.

---

## 🚀 Features

- Communicates with Dobiss lighting system via TCP
- Publishes light state and control topics to MQTT for Home Assistant integration
- Supports multiple Dobiss module types: `RELAY`, `DIMMER`, `ZERO_TO_TEN_VOLT`
- Exposes a REST API for manual control or integration without MQTT
- Configurable polling interval to sync light statuses
- Lightweight Docker container support

---

## ⚙️ Configuration

Configuration is provided through `appsettings.json`. Below is a sample:

```json
{
  "dobiss": {
    "Delay": 5000,
    "Modules": {
      "1": "RELAY",
      "2": "DIMMER",
      "3": "RELAY"
    },
    "DobissIp": "192.168.1.153",
    "DobissPort": 10001
  },
  "mqtt": {
    "Ip": "192.168.1.34",
    "Port": 1883,
    "User": "",
    "Password": ""
  }
}
````

### 🔌 `dobiss` Configuration

| Field        | Type   | Description                                                                             |
| ------------ | ------ | --------------------------------------------------------------------------------------- |
| `Delay`      | int    | Polling interval in milliseconds. Set to `0` to disable status polling.                 |
| `Modules`    | object | Map of module numbers to their types. Supported: `RELAY`, `DIMMER`, `ZERO_TO_TEN_VOLT`. |
| `DobissIp`   | string | IP address of the Dobiss controller.                                                    |
| `DobissPort` | int    | Port number of the Dobiss controller (default: `10001`).                                |

### 📡 `mqtt` Configuration (Optional)

| Field      | Type   | Description                                       |
| ---------- | ------ | ------------------------------------------------- |
| `Ip`       | string | IP address of the MQTT broker.                    |
| `Port`     | int    | MQTT port (default: `1883`).                      |
| `User`     | string | MQTT username (leave empty for anonymous access). |
| `Password` | string | MQTT password (leave empty for anonymous access). |

---

## 🧪 API

The REST API is available if enabled in your environment and provides endpoints to interact with the light system.

| Method | Endpoint                            | Description                        |
| ------ | ----------------------------------- | ---------------------------------- |
| GET    | `/light`                            | Returns a list of all light states |
| GET    | `/light/{module}/{key}`             | Get status of a specific light     |
| POST   | `/light/{module}/{key}/toggle`      | Toggle a light on/off              |
| POST   | `/light/{module}/{key}/set/{value}` | Set light state (0 = off, 1 = on)  |
| POST   | `/light/{module}/{key}/dim/{value}` | Set dim level (0–100 for DIMMER)   |

> Replace `{module}` and `{key}` with the correct identifiers for your Dobiss modules and outputs.

---

## ▶️ Usage

1. **Clone the repository**

   ```bash
   git clone https://github.com/vandenboschvincent/dobissconnector.git
   cd dobiss-mqtt-bridge
   ```

2. **Configure the app**
   Edit `appsettings.json` to reflect your Dobiss system and MQTT broker.

3. **Build and run**


### 🐳 Docker

To run using Docker:

```bash
docker run -d \
  --name dobiss-connector \
  -v /path/to/your/appsettings.json:/app/appsettings.json \
  -p 5000:80 \
  vandenboschvincent/dobissconnectorserviceapp
```

* Replace `/path/to/your/appsettings.json` with the full path to your local config.
* Ensure port `5000` (or any mapped port) is exposed if you plan to access the API.

---


## 📄 License

```
MIT License

Copyright (c) [year] [fullname]

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

