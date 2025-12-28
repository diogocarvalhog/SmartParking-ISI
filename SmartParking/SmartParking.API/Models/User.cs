// -----------------------------------------------------------------------------
// Projeto: SmartParking
// Unidade Curricular: ISI (IPCA)
// Autor: Diogo Graça
// Ficheiro: User.cs
// Descrição: Entidade de utilizador para autenticação/autorização via JWT.
// Notas:
//  - Password está em texto simples (adequado ao contexto académico, mas deve ser referido).
//  - Role suporta controlo de permissões (ex.: Admin vs User).
// -----------------------------------------------------------------------------

namespace SmartParking.API.Models
{
    #region Entidade: User

    /// <summary>
    /// Representa um utilizador do sistema.
    /// </summary>
    public class User
    {
        #region Propriedades (Persistência)

        /// <summary>
        /// Identificador único do utilizador.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nome de utilizador (credencial de login).
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Palavra-passe do utilizador.
        /// Nota: em produção, deveria ser armazenada como hash (com salt).
        /// </summary>
        public string Password { get; set; } = string.Empty; // Em produção, isto seria um Hash!

        /// <summary>
        /// Papel/perfil do utilizador (ex.: "User", "Admin").
        /// </summary>
        public string Role { get; set; } = "User";

        #endregion
    }

    #endregion
}