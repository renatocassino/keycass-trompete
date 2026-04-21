/** Vozes anunciadas como português do Brasil. */
export function getPtBrVoices(): SpeechSynthesisVoice[] {
	if (typeof speechSynthesis === 'undefined') return [];
	return speechSynthesis.getVoices().filter((v) => {
		return v.lang.startsWith('pt');
	});
}

export function initPtBrVoiceSelect(selectId: string): void {
	const select = document.getElementById(selectId);
	if (!select || !(select instanceof HTMLSelectElement)) return;

	const render = () => {
		const voices = getPtBrVoices();
		select.replaceChildren();

		if (voices.length === 0) {
			const opt = document.createElement('option');
			opt.value = '';
			opt.disabled = true;
			opt.selected = true;
			opt.textContent =
				typeof speechSynthesis !== 'undefined'
					? 'A carregar vozes…'
					: 'Web Speech API não disponível neste ambiente';
			select.appendChild(opt);
			return;
		}

		for (const v of voices) {
			const opt = document.createElement('option');
			opt.value = v.voiceURI;
			const suffix = v.localService ? '' : ' · nuvem';
			const def = v.default ? ' · padrão' : '';
			opt.textContent = `${v.name}${suffix}${def}`;
			select.appendChild(opt);
		}
		select.disabled = false;
	};

	select.disabled = true;
	render();
	speechSynthesis.addEventListener('voiceschanged', render);
}
