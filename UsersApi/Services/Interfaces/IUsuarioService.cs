using UsersApi.Dtos.Request;
using UsersApi.Dtos.Response;

namespace UsersApi.Services.Interfaces
{
    public interface IUsuarioService
    {
        Task<List<UsuarioResponseDto>> GetAllAsync();
        Task<UsuarioResponseDto> GetByIdAsync(int id);
        Task<UsuarioResponseDto> CreateAsync(UsuarioCreateRequestDto requestDto);
        Task<UsuarioResponseDto> UpdateAsync(int id, UsuarioUpdateRequestDto requestDto);
    }
}