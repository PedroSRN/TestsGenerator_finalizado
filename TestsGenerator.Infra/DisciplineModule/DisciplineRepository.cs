using FluentValidation;
using FluentValidation.Results;
using TestsGenerator.Domain.DisciplineModule;
using TestsGenerator.Infra.MateriaModule;
using TestsGenerator.Infra.Shared;

namespace TestsGenerator.Infra.DisciplineModule
{
    public class DisciplineRepository : BaseRepository<Discipline>
    {
        private MateriaRepository _materiaRepository;

        public DisciplineRepository(DataContext dataContext) : base(dataContext) 
        {
            if (dataContext.Disciplines.Count > 0)
            {
                this.counter = dataContext.Disciplines.Max(x => x.Id);
            }
        }

        public DisciplineRepository(DataContext dataContext, MateriaRepository materiaRepository) : this(dataContext)
        {
            _materiaRepository = materiaRepository;
        }

        public override List<Discipline> GetRegisters()
        {
            return _dataContext.Disciplines;
        }

        public override AbstractValidator<Discipline> GetValidator()
        {
            return new DisciplineValidator();
        }
        public override ValidationResult Insert(Discipline t)
        {
            bool nomeJaCadastrado = _dataContext.Disciplines.Any(x => x.Name.ToUpper() == t.Name.ToUpper());

            if (nomeJaCadastrado)
            {
                ValidationResult validadorNome = new ValidationResult();

                validadorNome.Errors.Add(new ValidationFailure("", "Registro não inserido, disciplina já cadastrada."));

                return validadorNome;
            }

            return base.Insert(t);
        }
        public ValidationResult Delete(Discipline t)
        {
            ValidationResult validationResult = GetValidator().Validate(t);


            if (_materiaRepository.GetRegisters().Select(x => x.Discipline).Contains(t))
                validationResult.Errors.Add(new ValidationFailure("", "Não é possível remover esta disciplina, pois ela está relacionada a uma materia."));

            return validationResult;
        }
        public override ValidationResult Update(Discipline t)
        {

            AbstractValidator<Discipline> validator = GetValidator();

            ValidationResult validationResult = validator.Validate(t);

            if (validationResult.IsValid)
            {
                List<Discipline> registers = GetRegisters();

                bool existsName = _dataContext.Disciplines.Any(x => x.Name.ToUpper() == t.Name.ToUpper());

                if (existsName)
                    validationResult.Errors.Add(new ValidationFailure("", "Nome já está cadastrado"));

                if (validationResult.IsValid)
                {
                    foreach(Discipline discipline in registers)
                    {
                        if(discipline.Id == t.Id)
                        {
                            discipline.Update(t);
                            break;
                        }
                    }
                }
            }

            return validationResult;
        }
    }
}