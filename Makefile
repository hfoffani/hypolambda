
MONOPATH=/Library/Frameworks/Mono.framework/Libraries/mono/4.5/
MONO=/Library/Frameworks/Mono.framework/Versions/Current/bin/mono
NUNITCONSOLE=packages/NUnit.Console.3.*/tools/nunit3-console.exe
TESTRES=--noresult

XAMARINDIR="/Applications/Xamarin Studio.app"
MDTOOL=$(XAMARINDIR)/Contents/MacOS/mdtool

NUGET=nuget


build:
	$(MDTOOL) build

test: build
	MONO_PATH=$(MONOPATH) $(MONO) $(NUNITCONSOLE) $(TESTRES) Tests/Tests.csproj
	
pack: test
	(cd HypoLambda && \
	rm HypoLambda.*.nupkg && \
	$(NUGET) pack HypoLambda.nuspec )

