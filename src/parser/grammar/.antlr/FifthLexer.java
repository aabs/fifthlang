// Generated from c:\dev\aabs\fifthlang\fifth.parser\grammar\Fifth.g4 by ANTLR 4.9.2
import org.antlr.v4.runtime.Lexer;
import org.antlr.v4.runtime.CharStream;
import org.antlr.v4.runtime.Token;
import org.antlr.v4.runtime.TokenStream;
import org.antlr.v4.runtime.*;
import org.antlr.v4.runtime.atn.*;
import org.antlr.v4.runtime.dfa.DFA;
import org.antlr.v4.runtime.misc.*;

@SuppressWarnings({"all", "warnings", "unchecked", "unused", "cast"})
public class FifthLexer extends Lexer {
	static { RuntimeMetaData.checkVersion("4.9.2", RuntimeMetaData.VERSION); }

	protected static final DFA[] _decisionToDFA;
	protected static final PredictionContextCache _sharedContextCache =
		new PredictionContextCache();
	public static final int
		ALIAS=1, AS=2, CLASS=3, ELSE=4, FALSE=5, IF=6, LIST=7, NEW=8, RETURN=9, 
		USE=10, TRUE=11, WHILE=12, WITH=13, AMP=14, AND=15, ASSIGN=16, BAR=17, 
		CLOSEBRACE=18, CLOSEBRACK=19, CLOSEPAREN=20, COLON=21, COMMA=22, DIVIDE=23, 
		DOT=24, EQ=25, GEN=26, GEQ=27, GT=28, HASH=29, LAMBDASEP=30, LEQ=31, LT=32, 
		MINUS=33, NEQ=34, NOT=35, OPENBRACE=36, OPENBRACK=37, OPENPAREN=38, OR=39, 
		PERCENT=40, PLUS=41, POWER=42, QMARK=43, SEMICOLON=44, TIMES=45, UNDERSCORE=46, 
		IDENTIFIER=47, STRING=48, INT=49, FLOAT=50, WS=51;
	public static String[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static String[] modeNames = {
		"DEFAULT_MODE"
	};

	private static String[] makeRuleNames() {
		return new String[] {
			"ALIAS", "AS", "CLASS", "ELSE", "FALSE", "IF", "LIST", "NEW", "RETURN", 
			"USE", "TRUE", "WHILE", "WITH", "AMP", "AND", "ASSIGN", "BAR", "CLOSEBRACE", 
			"CLOSEBRACK", "CLOSEPAREN", "COLON", "COMMA", "DIVIDE", "DOT", "EQ", 
			"GEN", "GEQ", "GT", "HASH", "LAMBDASEP", "LEQ", "LT", "MINUS", "NEQ", 
			"NOT", "OPENBRACE", "OPENBRACK", "OPENPAREN", "OR", "PERCENT", "PLUS", 
			"POWER", "QMARK", "SEMICOLON", "TIMES", "UNDERSCORE", "IDENTIFIER", "LETTER", 
			"STRING", "EXP", "DIGIT", "HEXDIGIT", "POSITIVEDIGIT", "INT", "FLOAT", 
			"WS"
		};
	}
	public static final String[] ruleNames = makeRuleNames();

	private static String[] makeLiteralNames() {
		return new String[] {
			null, "'alias'", "'as'", "'class'", "'else'", "'false'", "'if'", "'list'", 
			"'new'", "'return'", "'use'", "'true'", "'while'", "'with'", "'&'", "'&&'", 
			"'='", "'|'", "'}'", "']'", "')'", "':'", "','", "'/'", "'.'", "'=='", 
			"'<-'", "'>='", "'>'", "'#'", "'=>'", "'<='", "'<'", "'-'", "'!='", "'!'", 
			"'{'", "'['", "'('", "'||'", "'%'", "'+'", "'^'", "'?'", "';'", "'*'", 
			"'_'"
		};
	}
	private static final String[] _LITERAL_NAMES = makeLiteralNames();
	private static String[] makeSymbolicNames() {
		return new String[] {
			null, "ALIAS", "AS", "CLASS", "ELSE", "FALSE", "IF", "LIST", "NEW", "RETURN", 
			"USE", "TRUE", "WHILE", "WITH", "AMP", "AND", "ASSIGN", "BAR", "CLOSEBRACE", 
			"CLOSEBRACK", "CLOSEPAREN", "COLON", "COMMA", "DIVIDE", "DOT", "EQ", 
			"GEN", "GEQ", "GT", "HASH", "LAMBDASEP", "LEQ", "LT", "MINUS", "NEQ", 
			"NOT", "OPENBRACE", "OPENBRACK", "OPENPAREN", "OR", "PERCENT", "PLUS", 
			"POWER", "QMARK", "SEMICOLON", "TIMES", "UNDERSCORE", "IDENTIFIER", "STRING", 
			"INT", "FLOAT", "WS"
		};
	}
	private static final String[] _SYMBOLIC_NAMES = makeSymbolicNames();
	public static final Vocabulary VOCABULARY = new VocabularyImpl(_LITERAL_NAMES, _SYMBOLIC_NAMES);

	/**
	 * @deprecated Use {@link #VOCABULARY} instead.
	 */
	@Deprecated
	public static final String[] tokenNames;
	static {
		tokenNames = new String[_SYMBOLIC_NAMES.length];
		for (int i = 0; i < tokenNames.length; i++) {
			tokenNames[i] = VOCABULARY.getLiteralName(i);
			if (tokenNames[i] == null) {
				tokenNames[i] = VOCABULARY.getSymbolicName(i);
			}

			if (tokenNames[i] == null) {
				tokenNames[i] = "<INVALID>";
			}
		}
	}

	@Override
	@Deprecated
	public String[] getTokenNames() {
		return tokenNames;
	}

	@Override

	public Vocabulary getVocabulary() {
		return VOCABULARY;
	}


	public FifthLexer(CharStream input) {
		super(input);
		_interp = new LexerATNSimulator(this,_ATN,_decisionToDFA,_sharedContextCache);
	}

	@Override
	public String getGrammarFileName() { return "Fifth.g4"; }

	@Override
	public String[] getRuleNames() { return ruleNames; }

	@Override
	public String getSerializedATN() { return _serializedATN; }

	@Override
	public String[] getChannelNames() { return channelNames; }

	@Override
	public String[] getModeNames() { return modeNames; }

	@Override
	public ATN getATN() { return _ATN; }

	public static final String _serializedATN =
		"\3\u608b\ua72a\u8133\ub9ed\u417c\u3be7\u7786\u5964\2\65\u014b\b\1\4\2"+
		"\t\2\4\3\t\3\4\4\t\4\4\5\t\5\4\6\t\6\4\7\t\7\4\b\t\b\4\t\t\t\4\n\t\n\4"+
		"\13\t\13\4\f\t\f\4\r\t\r\4\16\t\16\4\17\t\17\4\20\t\20\4\21\t\21\4\22"+
		"\t\22\4\23\t\23\4\24\t\24\4\25\t\25\4\26\t\26\4\27\t\27\4\30\t\30\4\31"+
		"\t\31\4\32\t\32\4\33\t\33\4\34\t\34\4\35\t\35\4\36\t\36\4\37\t\37\4 \t"+
		" \4!\t!\4\"\t\"\4#\t#\4$\t$\4%\t%\4&\t&\4\'\t\'\4(\t(\4)\t)\4*\t*\4+\t"+
		"+\4,\t,\4-\t-\4.\t.\4/\t/\4\60\t\60\4\61\t\61\4\62\t\62\4\63\t\63\4\64"+
		"\t\64\4\65\t\65\4\66\t\66\4\67\t\67\48\t8\49\t9\3\2\3\2\3\2\3\2\3\2\3"+
		"\2\3\3\3\3\3\3\3\4\3\4\3\4\3\4\3\4\3\4\3\5\3\5\3\5\3\5\3\5\3\6\3\6\3\6"+
		"\3\6\3\6\3\6\3\7\3\7\3\7\3\b\3\b\3\b\3\b\3\b\3\t\3\t\3\t\3\t\3\n\3\n\3"+
		"\n\3\n\3\n\3\n\3\n\3\13\3\13\3\13\3\13\3\f\3\f\3\f\3\f\3\f\3\r\3\r\3\r"+
		"\3\r\3\r\3\r\3\16\3\16\3\16\3\16\3\16\3\17\3\17\3\20\3\20\3\20\3\21\3"+
		"\21\3\22\3\22\3\23\3\23\3\24\3\24\3\25\3\25\3\26\3\26\3\27\3\27\3\30\3"+
		"\30\3\31\3\31\3\32\3\32\3\32\3\33\3\33\3\33\3\34\3\34\3\34\3\35\3\35\3"+
		"\36\3\36\3\37\3\37\3\37\3 \3 \3 \3!\3!\3\"\3\"\3#\3#\3#\3$\3$\3%\3%\3"+
		"&\3&\3\'\3\'\3(\3(\3(\3)\3)\3*\3*\3+\3+\3,\3,\3-\3-\3.\3.\3/\3/\3\60\3"+
		"\60\5\60\u0101\n\60\3\60\3\60\3\60\7\60\u0106\n\60\f\60\16\60\u0109\13"+
		"\60\3\61\3\61\3\62\3\62\7\62\u010f\n\62\f\62\16\62\u0112\13\62\3\62\3"+
		"\62\3\62\7\62\u0117\n\62\f\62\16\62\u011a\13\62\3\62\5\62\u011d\n\62\3"+
		"\63\3\63\5\63\u0121\n\63\3\63\3\63\3\64\3\64\3\65\3\65\3\66\3\66\3\67"+
		"\3\67\5\67\u012d\n\67\3\67\3\67\7\67\u0131\n\67\f\67\16\67\u0134\13\67"+
		"\5\67\u0136\n\67\38\58\u0139\n8\38\38\38\68\u013e\n8\r8\168\u013f\38\5"+
		"8\u0143\n8\39\69\u0146\n9\r9\169\u0147\39\39\2\2:\3\3\5\4\7\5\t\6\13\7"+
		"\r\b\17\t\21\n\23\13\25\f\27\r\31\16\33\17\35\20\37\21!\22#\23%\24\'\25"+
		")\26+\27-\30/\31\61\32\63\33\65\34\67\359\36;\37= ?!A\"C#E$G%I&K\'M(O"+
		")Q*S+U,W-Y.[/]\60_\61a\2c\62e\2g\2i\2k\2m\63o\64q\65\3\2\13\4\2C\\c|\3"+
		"\2$$\3\2))\4\2GGgg\4\2--//\3\2\62;\5\2\62;CHch\3\2\63;\5\2\13\f\17\17"+
		"\"\"\2\u0154\2\3\3\2\2\2\2\5\3\2\2\2\2\7\3\2\2\2\2\t\3\2\2\2\2\13\3\2"+
		"\2\2\2\r\3\2\2\2\2\17\3\2\2\2\2\21\3\2\2\2\2\23\3\2\2\2\2\25\3\2\2\2\2"+
		"\27\3\2\2\2\2\31\3\2\2\2\2\33\3\2\2\2\2\35\3\2\2\2\2\37\3\2\2\2\2!\3\2"+
		"\2\2\2#\3\2\2\2\2%\3\2\2\2\2\'\3\2\2\2\2)\3\2\2\2\2+\3\2\2\2\2-\3\2\2"+
		"\2\2/\3\2\2\2\2\61\3\2\2\2\2\63\3\2\2\2\2\65\3\2\2\2\2\67\3\2\2\2\29\3"+
		"\2\2\2\2;\3\2\2\2\2=\3\2\2\2\2?\3\2\2\2\2A\3\2\2\2\2C\3\2\2\2\2E\3\2\2"+
		"\2\2G\3\2\2\2\2I\3\2\2\2\2K\3\2\2\2\2M\3\2\2\2\2O\3\2\2\2\2Q\3\2\2\2\2"+
		"S\3\2\2\2\2U\3\2\2\2\2W\3\2\2\2\2Y\3\2\2\2\2[\3\2\2\2\2]\3\2\2\2\2_\3"+
		"\2\2\2\2c\3\2\2\2\2m\3\2\2\2\2o\3\2\2\2\2q\3\2\2\2\3s\3\2\2\2\5y\3\2\2"+
		"\2\7|\3\2\2\2\t\u0082\3\2\2\2\13\u0087\3\2\2\2\r\u008d\3\2\2\2\17\u0090"+
		"\3\2\2\2\21\u0095\3\2\2\2\23\u0099\3\2\2\2\25\u00a0\3\2\2\2\27\u00a4\3"+
		"\2\2\2\31\u00a9\3\2\2\2\33\u00af\3\2\2\2\35\u00b4\3\2\2\2\37\u00b6\3\2"+
		"\2\2!\u00b9\3\2\2\2#\u00bb\3\2\2\2%\u00bd\3\2\2\2\'\u00bf\3\2\2\2)\u00c1"+
		"\3\2\2\2+\u00c3\3\2\2\2-\u00c5\3\2\2\2/\u00c7\3\2\2\2\61\u00c9\3\2\2\2"+
		"\63\u00cb\3\2\2\2\65\u00ce\3\2\2\2\67\u00d1\3\2\2\29\u00d4\3\2\2\2;\u00d6"+
		"\3\2\2\2=\u00d8\3\2\2\2?\u00db\3\2\2\2A\u00de\3\2\2\2C\u00e0\3\2\2\2E"+
		"\u00e2\3\2\2\2G\u00e5\3\2\2\2I\u00e7\3\2\2\2K\u00e9\3\2\2\2M\u00eb\3\2"+
		"\2\2O\u00ed\3\2\2\2Q\u00f0\3\2\2\2S\u00f2\3\2\2\2U\u00f4\3\2\2\2W\u00f6"+
		"\3\2\2\2Y\u00f8\3\2\2\2[\u00fa\3\2\2\2]\u00fc\3\2\2\2_\u0100\3\2\2\2a"+
		"\u010a\3\2\2\2c\u011c\3\2\2\2e\u011e\3\2\2\2g\u0124\3\2\2\2i\u0126\3\2"+
		"\2\2k\u0128\3\2\2\2m\u0135\3\2\2\2o\u0138\3\2\2\2q\u0145\3\2\2\2st\7c"+
		"\2\2tu\7n\2\2uv\7k\2\2vw\7c\2\2wx\7u\2\2x\4\3\2\2\2yz\7c\2\2z{\7u\2\2"+
		"{\6\3\2\2\2|}\7e\2\2}~\7n\2\2~\177\7c\2\2\177\u0080\7u\2\2\u0080\u0081"+
		"\7u\2\2\u0081\b\3\2\2\2\u0082\u0083\7g\2\2\u0083\u0084\7n\2\2\u0084\u0085"+
		"\7u\2\2\u0085\u0086\7g\2\2\u0086\n\3\2\2\2\u0087\u0088\7h\2\2\u0088\u0089"+
		"\7c\2\2\u0089\u008a\7n\2\2\u008a\u008b\7u\2\2\u008b\u008c\7g\2\2\u008c"+
		"\f\3\2\2\2\u008d\u008e\7k\2\2\u008e\u008f\7h\2\2\u008f\16\3\2\2\2\u0090"+
		"\u0091\7n\2\2\u0091\u0092\7k\2\2\u0092\u0093\7u\2\2\u0093\u0094\7v\2\2"+
		"\u0094\20\3\2\2\2\u0095\u0096\7p\2\2\u0096\u0097\7g\2\2\u0097\u0098\7"+
		"y\2\2\u0098\22\3\2\2\2\u0099\u009a\7t\2\2\u009a\u009b\7g\2\2\u009b\u009c"+
		"\7v\2\2\u009c\u009d\7w\2\2\u009d\u009e\7t\2\2\u009e\u009f\7p\2\2\u009f"+
		"\24\3\2\2\2\u00a0\u00a1\7w\2\2\u00a1\u00a2\7u\2\2\u00a2\u00a3\7g\2\2\u00a3"+
		"\26\3\2\2\2\u00a4\u00a5\7v\2\2\u00a5\u00a6\7t\2\2\u00a6\u00a7\7w\2\2\u00a7"+
		"\u00a8\7g\2\2\u00a8\30\3\2\2\2\u00a9\u00aa\7y\2\2\u00aa\u00ab\7j\2\2\u00ab"+
		"\u00ac\7k\2\2\u00ac\u00ad\7n\2\2\u00ad\u00ae\7g\2\2\u00ae\32\3\2\2\2\u00af"+
		"\u00b0\7y\2\2\u00b0\u00b1\7k\2\2\u00b1\u00b2\7v\2\2\u00b2\u00b3\7j\2\2"+
		"\u00b3\34\3\2\2\2\u00b4\u00b5\7(\2\2\u00b5\36\3\2\2\2\u00b6\u00b7\7(\2"+
		"\2\u00b7\u00b8\7(\2\2\u00b8 \3\2\2\2\u00b9\u00ba\7?\2\2\u00ba\"\3\2\2"+
		"\2\u00bb\u00bc\7~\2\2\u00bc$\3\2\2\2\u00bd\u00be\7\177\2\2\u00be&\3\2"+
		"\2\2\u00bf\u00c0\7_\2\2\u00c0(\3\2\2\2\u00c1\u00c2\7+\2\2\u00c2*\3\2\2"+
		"\2\u00c3\u00c4\7<\2\2\u00c4,\3\2\2\2\u00c5\u00c6\7.\2\2\u00c6.\3\2\2\2"+
		"\u00c7\u00c8\7\61\2\2\u00c8\60\3\2\2\2\u00c9\u00ca\7\60\2\2\u00ca\62\3"+
		"\2\2\2\u00cb\u00cc\7?\2\2\u00cc\u00cd\7?\2\2\u00cd\64\3\2\2\2\u00ce\u00cf"+
		"\7>\2\2\u00cf\u00d0\7/\2\2\u00d0\66\3\2\2\2\u00d1\u00d2\7@\2\2\u00d2\u00d3"+
		"\7?\2\2\u00d38\3\2\2\2\u00d4\u00d5\7@\2\2\u00d5:\3\2\2\2\u00d6\u00d7\7"+
		"%\2\2\u00d7<\3\2\2\2\u00d8\u00d9\7?\2\2\u00d9\u00da\7@\2\2\u00da>\3\2"+
		"\2\2\u00db\u00dc\7>\2\2\u00dc\u00dd\7?\2\2\u00dd@\3\2\2\2\u00de\u00df"+
		"\7>\2\2\u00dfB\3\2\2\2\u00e0\u00e1\7/\2\2\u00e1D\3\2\2\2\u00e2\u00e3\7"+
		"#\2\2\u00e3\u00e4\7?\2\2\u00e4F\3\2\2\2\u00e5\u00e6\7#\2\2\u00e6H\3\2"+
		"\2\2\u00e7\u00e8\7}\2\2\u00e8J\3\2\2\2\u00e9\u00ea\7]\2\2\u00eaL\3\2\2"+
		"\2\u00eb\u00ec\7*\2\2\u00ecN\3\2\2\2\u00ed\u00ee\7~\2\2\u00ee\u00ef\7"+
		"~\2\2\u00efP\3\2\2\2\u00f0\u00f1\7\'\2\2\u00f1R\3\2\2\2\u00f2\u00f3\7"+
		"-\2\2\u00f3T\3\2\2\2\u00f4\u00f5\7`\2\2\u00f5V\3\2\2\2\u00f6\u00f7\7A"+
		"\2\2\u00f7X\3\2\2\2\u00f8\u00f9\7=\2\2\u00f9Z\3\2\2\2\u00fa\u00fb\7,\2"+
		"\2\u00fb\\\3\2\2\2\u00fc\u00fd\7a\2\2\u00fd^\3\2\2\2\u00fe\u0101\5a\61"+
		"\2\u00ff\u0101\5]/\2\u0100\u00fe\3\2\2\2\u0100\u00ff\3\2\2\2\u0101\u0107"+
		"\3\2\2\2\u0102\u0106\5a\61\2\u0103\u0106\5g\64\2\u0104\u0106\5]/\2\u0105"+
		"\u0102\3\2\2\2\u0105\u0103\3\2\2\2\u0105\u0104\3\2\2\2\u0106\u0109\3\2"+
		"\2\2\u0107\u0105\3\2\2\2\u0107\u0108\3\2\2\2\u0108`\3\2\2\2\u0109\u0107"+
		"\3\2\2\2\u010a\u010b\t\2\2\2\u010bb\3\2\2\2\u010c\u0110\7$\2\2\u010d\u010f"+
		"\n\3\2\2\u010e\u010d\3\2\2\2\u010f\u0112\3\2\2\2\u0110\u010e\3\2\2\2\u0110"+
		"\u0111\3\2\2\2\u0111\u0113\3\2\2\2\u0112\u0110\3\2\2\2\u0113\u011d\7$"+
		"\2\2\u0114\u0118\7)\2\2\u0115\u0117\n\4\2\2\u0116\u0115\3\2\2\2\u0117"+
		"\u011a\3\2\2\2\u0118\u0116\3\2\2\2\u0118\u0119\3\2\2\2\u0119\u011b\3\2"+
		"\2\2\u011a\u0118\3\2\2\2\u011b\u011d\7)\2\2\u011c\u010c\3\2\2\2\u011c"+
		"\u0114\3\2\2\2\u011dd\3\2\2\2\u011e\u0120\t\5\2\2\u011f\u0121\t\6\2\2"+
		"\u0120\u011f\3\2\2\2\u0120\u0121\3\2\2\2\u0121\u0122\3\2\2\2\u0122\u0123"+
		"\5m\67\2\u0123f\3\2\2\2\u0124\u0125\t\7\2\2\u0125h\3\2\2\2\u0126\u0127"+
		"\t\b\2\2\u0127j\3\2\2\2\u0128\u0129\t\t\2\2\u0129l\3\2\2\2\u012a\u0136"+
		"\7\62\2\2\u012b\u012d\7/\2\2\u012c\u012b\3\2\2\2\u012c\u012d\3\2\2\2\u012d"+
		"\u012e\3\2\2\2\u012e\u0132\5k\66\2\u012f\u0131\5g\64\2\u0130\u012f\3\2"+
		"\2\2\u0131\u0134\3\2\2\2\u0132\u0130\3\2\2\2\u0132\u0133\3\2\2\2\u0133"+
		"\u0136\3\2\2\2\u0134\u0132\3\2\2\2\u0135\u012a\3\2\2\2\u0135\u012c\3\2"+
		"\2\2\u0136n\3\2\2\2\u0137\u0139\7/\2\2\u0138\u0137\3\2\2\2\u0138\u0139"+
		"\3\2\2\2\u0139\u013a\3\2\2\2\u013a\u013b\5m\67\2\u013b\u013d\7\60\2\2"+
		"\u013c\u013e\5g\64\2\u013d\u013c\3\2\2\2\u013e\u013f\3\2\2\2\u013f\u013d"+
		"\3\2\2\2\u013f\u0140\3\2\2\2\u0140\u0142\3\2\2\2\u0141\u0143\5e\63\2\u0142"+
		"\u0141\3\2\2\2\u0142\u0143\3\2\2\2\u0143p\3\2\2\2\u0144\u0146\t\n\2\2"+
		"\u0145\u0144\3\2\2\2\u0146\u0147\3\2\2\2\u0147\u0145\3\2\2\2\u0147\u0148"+
		"\3\2\2\2\u0148\u0149\3\2\2\2\u0149\u014a\b9\2\2\u014ar\3\2\2\2\21\2\u0100"+
		"\u0105\u0107\u0110\u0118\u011c\u0120\u012c\u0132\u0135\u0138\u013f\u0142"+
		"\u0147\3\b\2\2";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}