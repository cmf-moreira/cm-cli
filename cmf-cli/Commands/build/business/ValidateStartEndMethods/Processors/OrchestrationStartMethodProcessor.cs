﻿using Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Abstractions.Processors;
using Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Extensions;
using Cmf.CLI.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Processors;

internal class OrchestrationStartMethodProcessor : BaseProcessor, IOrchestrationStartMethodProcessor
{
	public override bool IsStartMethod => true;

	public override void Process()
	{
		base.Process();

		var statementArguments = _statementToProcess.DescendantNodes().OfType<ArgumentListSyntax>().FirstOrDefault();

		if (statementArguments is null)
		{
			return;
		}

		// TODO: Validate namespaceAndClass parameter

		if (statementArguments.Arguments[1] is null)
			Log.Warning(string.Format(ErrorMessages.StartMethodDoesNotContainMethodName, _className, _methodName));

		if (!statementArguments.Arguments[1].ToString().Trim('"').EqualsToItselfOrNameOfItself(_methodName))
			Log.Warning(string.Format(ErrorMessages.StartMethodMethodNameIsIncorrect, _className, _methodName));
	}

	public override void ValidateParameterCount(ArgumentListSyntax statementArguments)
	{
		if (statementArguments.Arguments.Count != 2 + _parameterNames.Count)
		{
			Log.Warning(string.Format(ErrorMessages.MethodHasIncorrectNumberOfParameters, StartEndMethodString, _className, _methodName, statementArguments.Arguments.Count, 2 + _parameterNames.Count));
		}
	}
}