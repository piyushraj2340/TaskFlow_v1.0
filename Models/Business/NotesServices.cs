using AutoMapper;
using Microsoft.CodeAnalysis.Operations;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Repositories;
using TaskMonitoringApp.Models.Services;

namespace TaskMonitoringApp.Models.Business
{
    public class NotesServices(IMapper mapper, INotesRepository repository) : INotesServices
    {
        private readonly INotesRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task AddNotesWithGoalId(string userId, NoteDTO notes, int goalId)
        {
            if(userId != notes.UserId)
            {
                throw new InvalidOperationException("userId and notes.UserId must be same!");
            }

            var notesToAdd = _mapper.Map<Notes>(notes);
            notesToAdd.GoalId = goalId;
            notesToAdd.TimeStamp = DateTime.Now;

            await _repository.AddNotes(notesToAdd);
        }

        public async Task AddNotesWithTaskId(string userId, NoteDTO notes, int taskId)
        {
            if (userId != notes.UserId)
            {
                throw new InvalidOperationException("userId and notes.UserId must be same!");
            }

            var notesToAdd = _mapper.Map<Notes>(notes);
            notesToAdd.TaskId = taskId;
            notesToAdd.TimeStamp = DateTime.Now;

            await _repository.AddNotes(notesToAdd);
        }

        public async Task<IEnumerable<NoteDTO>> GetAllNotes(string userId, Status status)
        {
            return await _repository.GetAllNotesAsync<NoteDTO>(userId, status, ResponseDataMode.ModelDTO);
        }

        public async Task<IEnumerable<NoteDTOWithGoalDTO>> GetAllNotesByGoalId(string userId, int goalId, Status status)
        {
            return await _repository.GetAllNotesForGoalIdAsync<NoteDTOWithGoalDTO>(userId, goalId, status, ResponseDataMode.ModelDTO); 
        }

        public async Task<IEnumerable<NoteDTOWithTaskDTO>> GetAllNotesByTaskId(string userId, int taskId, Status status)
        {
            return await _repository.GetAllNotesForTaskIdAsync<NoteDTOWithTaskDTO>(userId, taskId, status, ResponseDataMode.ModelDTO); 
        }

        public async Task<IEnumerable<Notes>> GetAllNotesWithFullContext(string userId, Status status)
        {
            return await _repository.GetAllNotesAsync<Notes>(userId, status, ResponseDataMode.Model);
        }

        public async Task<NoteDTO> GetNoteById(string userId, int noteId)
        {
            return await _repository.GetNotesByIdAsync<NoteDTO>(userId, noteId, ResponseDataMode.ModelDTO);
        }

        public async Task<NoteDTOWithGoalDTO> GetNoteByIdByGoalId(string userId, int noteId, int goalId)
        {
            return await _repository.GetNotesByIdWithGoalIdAsync<NoteDTOWithGoalDTO>(userId, noteId, goalId, ResponseDataMode.ModelDTO);
        }

        public async Task<NoteDTOWithTaskDTO> GetNoteByIdByTaskId(string userId, int noteId, int taskId)
        {
            return await _repository.GetNotesByIdWithTaskIdAsync<NoteDTOWithTaskDTO>(userId, noteId, taskId, ResponseDataMode.ModelDTO);
        }

        public async Task<Notes> GetNoteByIdWithFullContext(string userId, int noteId)
        {
            return await _repository.GetNotesByIdAsync<Notes>(userId, noteId, ResponseDataMode.Model);
        }

        public async Task UpdateNotes(string userId, NoteDTO noteToUpdate)
        {
            var getNotes = await _repository.GetNotesByIdAsync<Notes>(userId, noteToUpdate.Id, ResponseDataMode.Model);

            if(noteToUpdate.Title != getNotes.Title || noteToUpdate.Content != getNotes.Content)
            {
                noteToUpdate.IsModified = true;
                noteToUpdate.ModifiedOn = DateTime.Now;
            }

            noteToUpdate.TimeStamp = getNotes.TimeStamp; // Do Not change the timestamp if we update the notes....

            _mapper.Map<NoteDTO, Notes>(noteToUpdate, getNotes);

            await _repository.UpdateNotes(userId, getNotes);
        }

        public async Task DeleteNotes(string userId, int noteId)
        {
            var noteData = await _repository.GetNotesByIdAsync<Notes>(userId, noteId, ResponseDataMode.Model);

            noteData.IsDeleted = true;
            noteData.DeletedOn = DateTime.Now;
            noteData.UpdatedOn = DateTime.Now;

            await _repository.UpdateNotes(userId, noteData);
            
        }
    }
}
