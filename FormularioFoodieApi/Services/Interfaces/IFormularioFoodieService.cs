using FormularioFoodieApi.Dtos.Request;
using FormularioFoodieApi.Dtos.Response;

namespace FormularioFoodieApi.Services.Interfaces
{
    public interface IFormularioFoodieService
    {
        Task<FormularioFoodieResponseDto> CreateAsync(int usuarioId, FormularioFoodieCreateRequestDto requestDto);
        Task<FormularioFoodieResponseDto> UpdateAsync(int id, FormularioFoodieUpdateRequestDto requestDto);
        Task<FormularioFoodieResponseDto> UpdateMyFormularioAsync(int usuarioId, FormularioFoodieUpdateRequestDto requestDto);
        Task<FormularioFoodieResponseDto?> GetByIdAsync(int id);
        Task<FormularioFoodieResponseDto?> GetByUsuarioIdAsync(int usuarioId);
        Task<List<FormularioFoodieResponseDto>> GetAllAsync();
        Task<List<FormularioFoodieResponseDto>> GetByEstadoAsync(string estado);
    }
}