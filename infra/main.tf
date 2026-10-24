# Entorno PREPROD de Historialcancha, declarado. No existe hasta que esto se aplica,
# y desaparece con `terraform destroy`. QA y PROD (creados a mano en el TP6) NO están acá.
#
# Las credenciales NO van en este archivo: los providers las leen del entorno
# (NEON_API_KEY, RENDER_API_KEY, RENDER_OWNER_ID).
#
# Sin bloque `backend`: el estado queda en infra/terraform.tfstate, en la máquina del
# que aplica, e ignorado por git (tiene contraseñas en texto plano).

terraform {
  required_version = ">= 1.5"

  required_providers {
    neon = {
      source  = "kislerdm/neon"
      version = "0.18.0" # fijada: el repo del provider está archivado y el oficial todavía no existe
    }
    render = {
      source  = "render-oss/render"
      version = "1.9.1"
    }
    random = {
      source  = "hashicorp/random"
      version = "3.7.2"
    }
  }
}

provider "neon" {}   # lee NEON_API_KEY
provider "render" {} # lee RENDER_API_KEY y RENDER_OWNER_ID

# --- La base de PREPROD: un proyecto de Neon propio, que se puede destruir entero
#     sin rozar el proyecto `historialcancha` de QA y PROD ---

resource "neon_project" "preprod" {
  name   = "historialcancha-preprod"
  org_id = var.neon_org_id # la API de Neon lo exige: la cuenta vive dentro de una organización

  region_id = var.neon_region # la misma región que la base de QA y PROD

  # El provider pide por defecto un día de historial; el tope del plan gratuito son seis horas.
  history_retention_seconds = 21600
}

resource "neon_role" "preprod" {
  project_id = neon_project.preprod.id
  branch_id  = neon_project.preprod.default_branch_id
  name       = "app_preprod_owner"
}

resource "neon_database" "preprod" {
  project_id = neon_project.preprod.id
  branch_id  = neon_project.preprod.default_branch_id
  name       = "app_preprod"
  owner_name = neon_role.preprod.name
}

# --- Los dos servicios de PREPROD en Render. En el plan gratuito Render sólo deja CREAR y
#     DESTRUIR: cualquier modificación muere con "maintenance mode can only be configured for
#     non-free tier services". Para cambiar algo: destroy + apply, o apply -replace=<recurso>. ---

# La api no arranca sin una clave JWT de al menos 32 caracteres. La genera Terraform: nadie la
# escribe ni la copia, y vive sólo en el estado. Cambia en cada recreación de preprod.
resource "random_password" "jwt" {
  length  = 64
  special = false
}

locals {
  nombre_api   = "historialcancha-api-preprod"
  nombre_front = "historialcancha-front-preprod"

  # La cadena de conexión se ARMA con lo que devolvió Neon. Nadie la copia y pega: ése fue el
  # riesgo más caro del TP6 (un entorno apuntando a la base de otro).
  conexion = "Host=${neon_project.preprod.database_host};Database=${neon_database.preprod.name};Username=${neon_role.preprod.name};Password=${neon_role.preprod.password};SSL Mode=Require"

  # La api exige al menos un origen CORS para arrancar. No puede salir de front.url: el front ya
  # depende de la api (BACKEND_URL) y sería un ciclo. Se arma con el NOMBRE del front. En la
  # práctica el navegador no usa CORS: el front llama a /api por su nginx, desde el mismo origen.
  origen_front = "https://${local.nombre_front}.onrender.com"
}

resource "render_web_service" "api" {
  name   = local.nombre_api
  plan   = "free"
  region = var.region

  runtime_source = {
    image = {
      image_url = var.image_repo_api # sin la etiqueta: va en su propio campo
      tag       = var.image_tag
    }
  }

  # Sin App__Commit: lo hornea la imagen (build-arg COMMIT). Declararlo acá lo pisaría y el
  # health informaría un commit que no es el que corre.
  env_vars = {
    ConnectionStrings__HistorialCancha = { value = local.conexion }
    Jwt__Key                           = { value = random_password.jwt.result }
    App__MiEquipo                      = { value = var.mi_equipo }
    Cors__OrigenesPermitidos__0        = { value = local.origen_front }
    ASPNETCORE_ENVIRONMENT             = { value = "Production" }
    Salud__TimeoutChequeoMs            = { value = "3000" }
  }
}

resource "render_web_service" "front" {
  name   = local.nombre_front
  plan   = "free"
  region = var.region

  runtime_source = {
    image = {
      image_url = var.image_repo_front
      tag       = var.image_tag
    }
  }

  env_vars = {
    # Sale de la URL de SU api, no de un copy-paste. Sin barra final: con barra, nginx manda
    # todo a / y cada llamada da 404.
    BACKEND_URL  = { value = trimsuffix(render_web_service.api.url, "/") }
    DNS_RESOLVER = { value = "8.8.8.8" }
  }
}
