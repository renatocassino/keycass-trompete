/** Espera até não haver fala em curso nem itens na fila (em vez de `cancel()`). */
function waitUntilSpeechSynthesisIdle(synth: SpeechSynthesis): Promise<void> {
	if (!synth.speaking && !synth.pending) {
		return Promise.resolve();
	}
	return new Promise((resolve) => {
		const id = window.setInterval(() => {
			if (!synth.speaking && !synth.pending) {
				window.clearInterval(id);
				resolve();
			}
		}, 50);
	});
}

export class Speaker {
	private readonly synth: SpeechSynthesis;

	constructor() {
		this.synth = window.speechSynthesis;
	}

	/**
	 * Espera a síntese ficar disponível, fala o texto e resolve quando esta frase termina.
	 * Chamadas em sequência com `await` respeitam a ordem sem cortar a anterior.
	 */
	async speak(expression: string): Promise<void> {
		await waitUntilSpeechSynthesisIdle(this.synth);

		const utt = new SpeechSynthesisUtterance(expression);
		utt.lang = 'pt-BR';

		const voices = this.synth.getVoices();
		const voiceURI = (window as any).Alpine?.store('speech')?.voiceURI as string | undefined;
		const voice = voiceURI ? (voices.find((v) => v.voiceURI === voiceURI) ?? null) : null;
		if (voice) {
			utt.voice = voice;
		}

		return new Promise((resolve, reject) => {
			utt.onend = () => resolve();
			utt.onerror = (event: SpeechSynthesisErrorEvent) => {
				const msg =
					typeof event.error === 'string' && event.error.length > 0
						? event.error
						: 'Speech synthesis failed';

				reject(new Error(msg));
			};
			this.synth.speak(utt);
		});
	}
}
