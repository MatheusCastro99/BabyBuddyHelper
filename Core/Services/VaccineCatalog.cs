using BabyBuddyHelper.Core.Interfaces;
using BabyBuddyHelper.Core.Models;
using System.Collections.ObjectModel;

namespace BabyBuddyHelper.Core.Services
{
    //Routine birth-to-6-years immunizations as approved in #41, excluding flu, COVID-19 and combination products.
    //Ordered by age of first dose. CVX codes are CDC's "unspecified formulation" codes, so one entry covers every brand.
    //Ids must never change: vaccination records point at them.
    public class VaccineCatalog : IVaccineCatalog
    {
        private static readonly ReadOnlyCollection<VaccineModel> _vaccines = new VaccineModel[]
        {
            new()
            {
                Id = new Guid("7b23f2ec-1a06-4bda-bbd3-554da7ada11d"),
                CvxCode = "45",
                Name = "Hepatitis B",
                Description = "Helps protect against hepatitis B, a virus that can harm the liver."
            },
            new()
            {
                Id = new Guid("f63b5d1c-5022-4417-a319-0113063141e7"),
                CvxCode = "315", //RSV monoclonal antibody (nirsevimab or clesrovimab), an antibody shot rather than a vaccine
                Name = "RSV protection",
                Description = "An antibody shot that helps protect babies from RSV, a common virus that can cause serious breathing problems."
            },
            new()
            {
                Id = new Guid("795c6d25-d0ce-4031-b229-682b37e0c0dc"),
                CvxCode = "122",
                Name = "Rotavirus",
                Description = "Helps protect against rotavirus, which causes severe diarrhea and vomiting in young children."
            },
            new()
            {
                Id = new Guid("5d9135fd-5283-47ab-9443-4f73444216ef"),
                CvxCode = "107",
                Name = "DTaP",
                Description = "Helps protect against diphtheria, tetanus and whooping cough."
            },
            new()
            {
                Id = new Guid("d94b728a-0954-4b84-8aa2-9626e2798371"),
                CvxCode = "17",
                Name = "Hib",
                Description = "Helps protect against Hib bacteria, which can cause meningitis and other serious infections."
            },
            new()
            {
                Id = new Guid("00f98c75-5cbc-4f5b-85c0-4cfcab0c298f"),
                CvxCode = "152",
                Name = "Pneumococcal (PCV)",
                Description = "Helps protect against pneumococcal bacteria, which can cause ear infections, pneumonia and meningitis."
            },
            new()
            {
                Id = new Guid("44c8e9d0-11ea-442b-8997-817b5f43ce63"),
                CvxCode = "89",
                Name = "Polio (IPV)",
                Description = "Helps protect against polio, a virus that can cause paralysis."
            },
            new()
            {
                Id = new Guid("57dec016-a90d-4fc3-a02a-04400e4946af"),
                CvxCode = "03", //No unspecified-formulation code exists for MMR
                Name = "MMR",
                Description = "Helps protect against measles, mumps and rubella."
            },
            new()
            {
                Id = new Guid("bcb6dce0-114f-4e95-9312-f892cae9ecb0"),
                CvxCode = "21", //No unspecified-formulation code exists for varicella
                Name = "Chickenpox (Varicella)",
                Description = "Helps protect against chickenpox."
            },
            new()
            {
                Id = new Guid("841b8d98-3be0-46c1-a89f-0297add6404e"),
                CvxCode = "85",
                Name = "Hepatitis A",
                Description = "Helps protect against hepatitis A, a liver virus that spreads through food and water."
            }
        }.AsReadOnly(); //Callers can't cast the list back to an array and swap entries

        public IReadOnlyList<VaccineModel> Vaccines => _vaccines;

        //The schedule in effect under the March 2026 court stay; re-check after the First Circuit ruling (#79)
        public string SourceName => "US CDC childhood immunization schedule";
        public string SourceVersion => "2025";
        public Uri SourceUrl { get; } = new("https://www.cdc.gov/vaccines/imz-schedules/child-easyread.html");
        public DateOnly LastReviewed => new(2026, 9, 28);

        public VaccineModel? GetById(Guid vaccineId) => _vaccines.FirstOrDefault(x => x.Id == vaccineId);
    }
}
