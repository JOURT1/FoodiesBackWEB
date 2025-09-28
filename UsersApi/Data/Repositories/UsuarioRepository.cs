using Microsoft.EntityFrameworkCore;
using UsersApi.Data.Repositories.Interfaces;
using UsersApi.Models;

namespace UsersApi.Data.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly UsersDbContext _context;

        public UsuarioRepository(UsersDbContext context)
        {
            _context = context;
        }

        public async Task<List<Usuario>> GetAllAsync()
        {
            return await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                    .ThenInclude(ur => ur.Rol)
                .Where(u => u.Activo)
                .OrderBy(u => u.Id)
                .ToListAsync();
        }

        public async Task<Usuario?> GetByIdAsync(int id)
        {
            return await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                    .ThenInclude(ur => ur.Rol)
                .FirstOrDefaultAsync(u => u.Id == id && u.Activo);
        }

        public async Task<Usuario?> GetByCorreoAsync(string correo)
        {
            return await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                    .ThenInclude(ur => ur.Rol)
                .FirstOrDefaultAsync(u => u.Correo == correo && u.Activo);
        }

        public async Task<Usuario> CreateAsync(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

        public async Task<Usuario> UpdateAsync(Usuario usuario)
        {
            usuario.FechaActualizacion = DateTime.UtcNow;
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

        public async Task<bool> ExisteCorreoAsync(string correo)
        {
            return await _context.Usuarios
                .AnyAsync(u => u.Correo == correo && u.Activo);
        }

        public async Task<bool> ExisteCorreoAsync(string correo, int excludeId)
        {
            return await _context.Usuarios
                .AnyAsync(u => u.Correo == correo && u.Id != excludeId && u.Activo);
        }
    }
}