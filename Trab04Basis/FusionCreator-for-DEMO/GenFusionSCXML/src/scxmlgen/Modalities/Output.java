package scxmlgen.Modalities;

import scxmlgen.interfaces.IOutput;



public enum Output implements IOutput{
    
	AVANCAR("[action][AvancarL]"),
	RECUAR("[action][RecuarR]"),
	ESVAZIARC("[action][EsvaziarC]"),
	VERC("[action][VerC]"),
	MCDONALDS("[restaurant][mcdonalds]"),
	MCDONALDS_UNIVERSIDADE("[restaurant][mcdonalds][place][UNIVERSIDADE]"),
	MONTADITOS("[restaurant][montaditos]"),
	SIM("[action][SIM]"),
	NAO("[action][NAO]"),
	scrollUpDevagar("[action][scrollUpDevagar]"),
	scrollUpRapido("[action][scrollUpRapido]"),
	scrollDownRapido("[action][scrollDownRapido]"),
	scrollDownDevagar("[action][scrollDownDevagar]"),;    
    
    
    private String event;

    Output(String m) {
        event=m;
    }
    
    public String getEvent(){
        return this.toString();
    }

    public String getEventName(){
        return event;
    }
}
