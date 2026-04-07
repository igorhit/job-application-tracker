using ApplicationTracker.Domain.Entities;
using ApplicationTracker.Domain.Enums;
using System.Globalization;

namespace ApplicationTracker.Application.Features.JobApplications.Queries.GenerateStudyAssistant;

internal static class StudyAssistantPromptBuilder
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public const string SystemPrompt =
        "Você é um assistente de carreira e entrevistas. Responda sempre em português do Brasil, " +
        "com objetividade, boa estrutura e foco em estudo rápido, preparação prática e clareza.";

    public static string BuildUserPrompt(JobApplication application, string mode)
    {
        return mode switch
        {
            StudyPromptModes.InterviewPrep => BuildInterviewPrepPrompt(application),
            StudyPromptModes.ReviewPlan => BuildReviewPlanPrompt(application),
            _ => BuildQuickStudyPrompt(application),
        };
    }

    private static string BuildQuickStudyPrompt(JobApplication application)
    {
        return string.Join('\n', [
            $"Estou me preparando para uma vaga de \"{application.JobTitle}\" na empresa \"{application.Company.Name}\".",
            string.Empty,
            "Contexto da vaga:",
            BuildContextBlock(application),
            string.Empty,
            "Principais requisitos mapeados:",
            BuildRequirementsSection(application),
            string.Empty,
            "Quero um estudo rápido e altamente prático para ficar funcional nessa vaga.",
            "Monte a resposta com:",
            "1. resumo objetivo do que preciso dominar",
            "2. explicação curta de cada requisito listado",
            "3. ordem de prioridade do que estudar primeiro",
            "4. checklist prático de revisão para hoje",
            "5. perguntas técnicas e comportamentais prováveis",
            "6. mini plano de estudo de 2 horas e de 1 dia",
            "7. sugestões de exercícios rápidos ou mini projeto para fixação",
            string.Empty,
            "Evite teoria excessiva e foque no que mais aumenta minha chance de performar bem em entrevista e desafio técnico.",
        ]);
    }

    private static string BuildInterviewPrepPrompt(JobApplication application)
    {
        return string.Join('\n', [
            $"Vou participar de um processo seletivo para a vaga \"{application.JobTitle}\" na empresa \"{application.Company.Name}\".",
            string.Empty,
            "Contexto da vaga:",
            BuildContextBlock(application),
            string.Empty,
            "Requisitos principais da vaga:",
            BuildRequirementsSection(application),
            string.Empty,
            "Quero uma preparação objetiva para entrevista técnica e comportamental.",
            "Monte a resposta com:",
            "1. o que o recrutador e o entrevistador provavelmente querem ouvir para cada requisito",
            "2. perguntas técnicas prováveis com respostas curtas e didáticas",
            "3. perguntas comportamentais prováveis com exemplos de estrutura de resposta",
            "4. armadilhas comuns e erros de comunicação que devo evitar",
            "5. pontos de experiência prática que eu devo destacar mesmo com pouca senioridade",
            "6. um roteiro de revisão de 30 minutos antes da entrevista",
            string.Empty,
            "Seja direto, específico e voltado para performance em entrevista.",
        ]);
    }

    private static string BuildReviewPlanPrompt(JobApplication application)
    {
        return string.Join('\n', [
            $"Preciso montar um plano enxuto de revisão para a vaga \"{application.JobTitle}\" da empresa \"{application.Company.Name}\".",
            string.Empty,
            "Contexto da vaga:",
            BuildContextBlock(application),
            string.Empty,
            "Requisitos principais da vaga:",
            BuildRequirementsSection(application),
            string.Empty,
            "Monte um plano de revisão com:",
            "1. blocos de estudo de 25 a 45 minutos por prioridade",
            "2. o que revisar hoje em 1 hora, 2 horas e 1 dia",
            "3. materiais ou tipos de recurso mais adequados para cada bloco",
            "4. exercícios ou validações rápidas para saber se aprendi o suficiente",
            "5. checklist final de revisão antes de entrevista ou desafio técnico",
            string.Empty,
            "Quero um plano pragmático, sem excesso de teoria e com foco em retenção rápida.",
        ]);
    }

    private static string BuildContextBlock(JobApplication application)
    {
        return string.Join('\n', [
            $"- Cargo: {application.JobTitle}",
            $"- Empresa: {application.Company.Name}",
            $"- Status atual da candidatura: {GetStatusLabel(application.Status)}",
            $"- Local: {application.Location ?? "Não informado"}",
            $"- URL da vaga: {application.JobUrl ?? "Não informada"}",
            $"- Pretensão salarial: {(application.SalaryExpectation is null ? "Não informada" : application.SalaryExpectation.Value.ToString("C", PtBr))}",
            $"- Próxima ação: {application.NextActionNote ?? "Não definida"}",
            $"- Data da próxima ação: {(application.NextActionAt is null ? "Não definida" : application.NextActionAt.Value.ToString("dd/MM/yyyy", PtBr))}",
        ]);
    }

    private static string BuildRequirementsSection(JobApplication application)
    {
        var requirements = application.Requirements
            .OrderBy(requirement => requirement.DisplayOrder)
            .Select((requirement, index) => $"{index + 1}. {requirement.Content}")
            .ToList();

        return requirements.Count > 0
            ? string.Join('\n', requirements)
            : "Nenhum requisito foi listado explicitamente.";
    }

    private static string GetStatusLabel(ApplicationStatus status)
    {
        return status switch
        {
            ApplicationStatus.Wishlist => "Wishlist",
            ApplicationStatus.Applied => "Aplicado",
            ApplicationStatus.Interview => "Entrevista",
            ApplicationStatus.Challenge => "Desafio",
            ApplicationStatus.Offer => "Oferta",
            ApplicationStatus.Rejected => "Rejeitado",
            _ => status.ToString(),
        };
    }
}
