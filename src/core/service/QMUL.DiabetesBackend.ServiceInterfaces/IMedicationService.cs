﻿namespace QMUL.DiabetesBackend.ServiceInterfaces;

using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Model;
using Model.Exceptions;

/// <summary>
/// The Medication Service Interface.
/// </summary>
public interface IMedicationService
{
    /// <summary>
    /// Gets a paginated list of the <see cref="Medication"/> objects stored. 
    /// </summary>
    /// <param name="paginationRequest">The pagination request parameter.</param>
    /// <param name="name">The medication display name to look for. Maps to the Coding property of the
    /// <see cref="Medication"/> object.</param>
    /// <returns>A <see cref="PaginatedResult{T}"/> of a <see cref="Bundle"/> object with the medication list.</returns>
    Task<PaginatedResult<Bundle>> GetMedicationList(PaginationRequest paginationRequest, string? name = null);

    /// <summary>
    /// Gets a single <see cref="Medication"/> given an ID.
    /// </summary>
    /// <param name="id">The <see cref="Medication"/> ID to look for.</param>
    /// <returns>A <see cref="Medication"/> object if found; null otherwise.</returns>
    Task<Medication?> GetMedication(string id);

    /// <summary>
    /// Creates a <see cref="Medication"/>.
    /// </summary>
    /// <param name="newMedication">The <see cref="Medication"/> object to create.</param>
    /// <returns>The created <see cref="Medication"/> with a new ID.</returns>
    /// <exception cref="WriteResourceException">If the medication was not created.</exception>
    Task<Medication> CreateMedication(Medication newMedication);
}