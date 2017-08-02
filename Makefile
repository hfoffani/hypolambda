
MONOPATH=/Library/Frameworks/Mono.framework/Libraries/mono/4.5/
MONO=/Library/Frameworks/Mono.framework/Versions/Current/bin/mono
NUNITCONSOLE=packages/NUnit.Console.3.*/tools/nunit3-console.exe
TESTRES=--noresult

XAMARINDIR="/Applications/Visual Studio.app"
MDTOOL=$(XAMARINDIR)/Contents/MacOS/vstool

NUGET=nuget


build:
	$(MDTOOL) build

test: build
	MONO_PATH=$(MONOPATH) $(MONO) $(NUNITCONSOLE) $(TESTRES) Tests/Tests.csproj
	
nupkg: test
	(cd HypoLambda && \
	rm -f HypoLambda.*.nupkg && \
	$(NUGET) pack HypoLambda.nuspec )

publish:
	(cd HypoLambda && \
	$(NUGET) push HypoLambda.*.nupkg )
