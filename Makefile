compile:
	qmk compile -kb keycass_trompete -km default

flash:
	qmk flash -kb keycass_trompete -km default

update:
	make compile flash

build:
	dotnet run build

install-qmk:
	python3 -m venv ./python-env
	./python-env/bin/pip install qmk

	echo "\n\nNow, please run: source ./python-env/bin/activate"
