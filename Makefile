
MONOPATH=/Library/Frameworks/Mono.framework/Libraries/mono/4.5/
MONO=/Library/Frameworks/Mono.framework/Versions/Current/bin/mono
NUNITCONSOLE=packages/NUnit.Console.3.0.0/tools/nunit3-console.exe
TESTRES=--noresult

XAMARINDIR="/Applications/Xamarin Studio.app"
MDTOOL=$(XAMARINDIR)/Contents/MacOS/mdtool


build:
	$(MDTOOL) build

test: build
	MONO_PATH=$(MONOPATH) $(MONO) $(NUNITCONSOLE) $(TESTRES) Tests/Tests.csproj
	
