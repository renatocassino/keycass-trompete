import { TextProcessor } from './textProcessor';

const textProcessor = new TextProcessor();

export function updateKeydown(event: KeyboardEvent): void {
	const currentLetter = event.key.toLowerCase();

	if (!currentLetter.match(/^[a-z ]$/)) {
		return;
	}

	textProcessor.push(currentLetter);

	const store = (window as any).Alpine?.store('speech') as
		| { voiceURI: string; lastKey: string; currentText: string }
		| undefined;

	if (store) {
		store.lastKey = currentLetter;
		store.currentText = textProcessor.getText();
	}
}
