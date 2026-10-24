# Lo configurable de preprod. Nada de esto es secreto, por eso va con default en un
# archivo versionado; lo que no se publica iría en un *.tfvars (ignorado por git).

variable "neon_org_id" {
  description = "Organización de Neon donde se crea el proyecto de preprod"
  default     = "org-withered-wildflower-33464219"
}

variable "neon_region" {
  description = "Región de la base: la misma que QA y PROD (São Paulo)"
  default     = "aws-sa-east-1"
}

variable "region" {
  description = "Región de Render: la misma que QA y PROD"
  default     = "virginia"
}

variable "image_repo_api" {
  description = "Imagen de la api, SIN la etiqueta"
  default     = "ghcr.io/nicolasbono/historialcancha-backend"
}

variable "image_repo_front" {
  description = "Imagen del front, SIN la etiqueta"
  default     = "ghcr.io/nicolasbono/historialcancha-frontend"
}

variable "image_tag" {
  description = "Etiqueta de las dos imágenes: la del merge de v7.0.0, lo que corre en PROD"
  default     = "sha-575a5a166c4444d5e4fe55c02d5b6a8a62c3579a"
}

variable "mi_equipo" {
  description = "El equipo del hincha: la api rechaza partidos contra él mismo"
  default     = "Boca Juniors"
}
